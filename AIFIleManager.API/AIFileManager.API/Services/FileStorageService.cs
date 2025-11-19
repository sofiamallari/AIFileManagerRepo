using AIFileManager.API.DTO;
using AIFileManager.DTO;
using AIFileManager.Interfaces;
using System.Security.Cryptography;
using VBIO = Microsoft.VisualBasic.FileIO;

namespace AIFileManager.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IAIService _aiService;

        public FileStorageService(IAIService aiService)
        {
            _aiService = aiService;
        }

        #region Drive / Folder / File Scanning

        public async Task<IEnumerable<DriveInfoDto>> GetDrivesAsync()
        {
            return await Task.Run(() =>
            {
                return DriveInfo.GetDrives()
                    .Where(d => d.IsReady)
                    .Select(d => new DriveInfoDto
                    {
                        DriveName = d.Name,
                        TotalSizeGB = Math.Round(d.TotalSize / (1024.0 * 1024 * 1024), 2),
                        AvailableSizeGB = Math.Round(d.AvailableFreeSpace / (1024.0 * 1024 * 1024), 2),
                        UsedSizeGB = Math.Round((d.TotalSize - d.AvailableFreeSpace) / (1024.0 * 1024 * 1024), 2)
                    })
                    .ToList();
            });
        }

        public async Task<IEnumerable<FolderInfoDto>> GetFoldersAsync(string path, bool deepScan = false)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"Directory not found: {path}");

            var results = new List<FolderInfoDto>();

            foreach (var folder in Directory.GetDirectories(path))
            {
                try
                {
                    var dirInfo = new DirectoryInfo(folder);

                    long sizeBytes = deepScan
                        ? await GetDirectorySizeParallelAsync(dirInfo, true)
                        : await GetDirectorySizeParallelAsync(dirInfo, false);

                    results.Add(new FolderInfoDto
                    {
                        Name = dirInfo.Name,
                        SizeMB = Math.Round(sizeBytes / (1024.0 * 1024), 2),
                        LastModified = dirInfo.LastWriteTime
                    });
                }
                catch { }
            }

            return results;
        }

        public async Task<IEnumerable<FileInfoDto>> GetFilesAsync(string path, bool deepScan = false)
        {
            return await EnumerateFilesAsync(path, includeHash: false, deepScan);
        }

        public async Task<IEnumerable<FileInfoDto>> GetFileMetadataAsync(string path, bool deepScan = false)
        {
            return await EnumerateFilesAsync(path, includeHash: true, deepScan);
        }

        public async Task<IEnumerable<FileInfoDto>> GetFolderFileMetadataAsync(string folderPath, bool deepScan = false)
        {
            return await EnumerateFilesAsync(folderPath, includeHash: true, deepScan);
        }

        #endregion

        #region File Operations

        public async Task<OperationResultDto> DeleteFileAsync(string path, bool permanent = false)
        {
            return await TryExecuteIOAsync(async () =>
            {
                if (permanent || !OperatingSystem.IsWindows())
                    File.Delete(path);
                else
                {
                    VBIO.FileSystem.DeleteFile(
                        path,
                        VBIO.UIOption.OnlyErrorDialogs,
                        VBIO.RecycleOption.SendToRecycleBin);
                }

                await Task.CompletedTask;
            }, permanent ? "File permanently deleted." : "File moved to Recycle Bin successfully.");
        }

        public async Task<OperationResultDto> DeleteFolderAsync(string path, bool permanent = false)
        {
            return await TryExecuteIOAsync(async () =>
            {
                if (permanent || !OperatingSystem.IsWindows())
                    Directory.Delete(path, true);
                else
                {
                    VBIO.FileSystem.DeleteDirectory(
                        path,
                        VBIO.UIOption.OnlyErrorDialogs,
                        VBIO.RecycleOption.SendToRecycleBin);
                }

                await Task.CompletedTask;
            }, permanent ? "Folder permanently deleted." : "Folder moved to Recycle Bin successfully.");
        }

        public async Task<OperationResultDto> MoveFileAsync(string source, string destination)
        {
            return await TryExecuteIOAsync(async () =>
            {
                var dest = Path.Combine(destination, Path.GetFileName(source));
                if (File.Exists(dest)) File.Delete(dest);

                File.Move(source, dest);
                await Task.CompletedTask;
            }, "File moved successfully.");
        }

        public async Task<OperationResultDto> MoveFolderAsync(string source, string destination)
        {
            return await TryExecuteIOAsync(async () =>
            {
                var dest = Path.Combine(destination, Path.GetFileName(source));
                if (Directory.Exists(dest)) Directory.Delete(dest, true);

                Directory.Move(source, dest);
                await Task.CompletedTask;
            }, "Folder moved successfully.");
        }

        #endregion

        #region Old Batch Analyzer (kept for compatibility)

        public async Task<AIAnalysisResultDto> AnalyzeFilesInBatchesAsync(IEnumerable<FileInfoDto> files, int batchSize = 10)
        {
            var list = files.ToList();

            if (!list.Any())
            {
                return new AIAnalysisResultDto
                {
                    Suggestions = new List<string> { "No files to analyze." }
                };
            }

            var final = new AIAnalysisResultDto
            {
                LargeFiles = new List<AnalysisFileInfoDto>(),
                DuplicateFiles = new List<AnalysisFileInfoDto>(),
                OldFiles = new List<AnalysisFileInfoDto>(),
                Suggestions = new List<string>()
            };

            var batches = list
                .Select((f, i) => new { f, i })
                .GroupBy(x => x.i / batchSize)
                .Select(g => g.Select(x => x.f).ToList())
                .ToList();

            foreach (var batch in batches)
            {
                try
                {
                    var r = await _aiService.AnalyzeFolderAsync(batch.Select(f => new FolderInfoDto()).ToList());
                    if (r != null)
                    {
                        final.LargeFiles.AddRange(r.LargeFiles ?? []);
                        final.DuplicateFiles.AddRange(r.DuplicateFiles ?? []);
                        final.OldFiles.AddRange(r.OldFiles ?? []);
                        final.Suggestions.AddRange(r.Suggestions ?? []);
                    }
                }
                catch { }
            }

            return final;
        }

        #endregion

        #region Parallel Batch Analyzer (Final Correct Version)

        public async Task<List<DecisionDto>> AnalyzeFilesParallelBatchesAsync(
     IEnumerable<FileInfoDto> files,
     int batchSize,
     Func<int, Task>? onBatchCompleted = null)
        {
            var fileList = files.ToList();

            var batches = fileList
                .Select((file, index) => new { file, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.file).ToList())
                .ToList();

            // This will collect ALL results from ALL batches
            var allBatchResults = new List<DecisionDto>();

            var batchTasks = batches.Select(async (batch, batchIndex) =>
            {
                try
                {
                    // 🔥 Call new Python batch endpoint
                    var batchResult = await _aiService.AnalyzeBatchAsync(batch);

                    if (batchResult != null && batchResult.Any())
                        allBatchResults.AddRange(batchResult);
                }
                catch (Exception ex)
                {
                    allBatchResults.Add(new DecisionDto
                    {
                        Action = "info",
                        Reason = $"Batch {batchIndex + 1} failed: {ex.Message}"
                    });
                }

                if (onBatchCompleted != null)
                    await onBatchCompleted(batchIndex + 1);
            });

            // Wait for all batches to finish
            await Task.WhenAll(batchTasks);

            return allBatchResults;
        }


        #endregion

        #region Helpers

        private async Task<List<FileInfoDto>> EnumerateFilesAsync(string path, bool includeHash, bool deepScan)
        {
            var results = new List<FileInfoDto>();

            var queue = new Queue<string>();
            queue.Enqueue(path);

            while (queue.Count > 0)
            {
                var folder = queue.Dequeue();

                if (deepScan)
                {
                    foreach (var d in Directory.GetDirectories(folder))
                        queue.Enqueue(d);
                }

                foreach (var file in Directory.GetFiles(folder))
                {
                    var info = new FileInfo(file);

                    var dto = new FileInfoDto
                    {
                        Name = info.Name,
                        SizeMB = Math.Round(info.Length / (1024.0 * 1024), 2),
                        ModifiedDate = info.LastWriteTime
                    };

                    if (includeHash)
                        dto.Hash = await ComputePartialHashAsync(file);

                    results.Add(dto);
                }
            }

            return results;
        }

        private static async Task<long> GetDirectorySizeParallelAsync(DirectoryInfo dir, bool recursive)
        {
            long total = 0;

            var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            await Parallel.ForEachAsync(dir.EnumerateFiles("*", option), async (f, _) =>
            {
                Interlocked.Add(ref total, f.Length);
                await Task.Yield();
            });

            return total;
        }

        private async Task<OperationResultDto> TryExecuteIOAsync(Func<Task> action, string success)
        {
            try
            {
                await action();
                return new OperationResultDto { Success = true, Message = success };
            }
            catch (Exception ex)
            {
                return new OperationResultDto { Success = false, Message = ex.Message };
            }
        }

        private static async Task<string> ComputePartialHashAsync(string file)
        {
            byte[] buffer = new byte[1024 * 256];

            using var md5 = MD5.Create();
            await using var stream = File.OpenRead(file);

            int read = await stream.ReadAsync(buffer);
            var hash = md5.ComputeHash(buffer, 0, read);

            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        #endregion
    }
}
