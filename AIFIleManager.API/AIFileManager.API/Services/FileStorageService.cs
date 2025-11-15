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
        #region private
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
                    }).ToList();
            });
        }

        public async Task<IEnumerable<FolderInfoDto>> GetFoldersAsync(string path, bool deepScan = false)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be null or empty.");

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"Directory not found: {path}");

            var folderList = new List<FolderInfoDto>();

            try
            {
                var directories = Directory.GetDirectories(path);

                foreach (var folder in directories)
                {
                    var dirInfo = new DirectoryInfo(folder);
                    //long sizeBytes = await GetDirectorySizeAsync(dirInfo, recursive: true);
                    // Choose recursive strategy based on deepScan flag
                    long sizeBytes = deepScan
                        ? await GetDirectorySizeParallelAsync(dirInfo, recursive: true)
                        : await GetDirectorySizeParallelAsync(dirInfo, recursive: false);

                    folderList.Add(new FolderInfoDto
                    {
                        Name = dirInfo.Name,
                        SizeMB = Math.Round(sizeBytes / (1024.0 * 1024), 2),
                        LastModified = dirInfo.LastWriteTime
                    });
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip folders we can’t access
            }

            return folderList;
        }

        public async Task<IEnumerable<FileInfoDto>> GetFilesAsync(string path, bool deepScan = false)
        {
            try
            {
                return await EnumerateFilesAsync(path, includeHash: false, deepScan);
            }
            catch (Exception ex)
            {
                throw new IOException($"Error listing files for path {path}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<FileInfoDto>> GetFileMetadataAsync(string path, bool deepScan = false)
        {
            try
            {
                return await EnumerateFilesAsync(path, includeHash: true, deepScan);
            }
            catch (Exception ex)
            {
                throw new IOException($"Error listing metadata for path {path}: {ex.Message}", ex);
            }
        }
        public async Task<IEnumerable<FileInfoDto>> GetFolderFileMetadataAsync(string folderPath, bool deepScan = false)
        {
            return await EnumerateFilesAsync(folderPath, includeHash: true, deepScan);
        }

        public async Task<OperationResultDto> DeleteFileAsync(string path, bool permanent = false)
        {
            return await TryExecuteIOAsync(async () =>
            {
                if (!File.Exists(path))
                    throw new FileNotFoundException("File not found.", path);
                if (!OperatingSystem.IsWindows())
                {
                    // Fallback for Docker/Linux: always permanent delete
                    File.Delete(path);
                }
                else
                {
                    if (permanent)
                    {
                        // Permanent delete
                        File.Delete(path);
                    }
                    else
                    {
                        if (OperatingSystem.IsWindows())
                        {
                            // Move to Recycle Bin (Windows only)
                            VBIO.FileSystem.DeleteFile(path,
                            VBIO.UIOption.OnlyErrorDialogs,
                            VBIO.RecycleOption.SendToRecycleBin);
                        }
                    }
                }
                await Task.CompletedTask;
            }, permanent ? "File permanently deleted." : "File moved to Recycle Bin successfully.");
        }

        public async Task<OperationResultDto> DeleteFolderAsync(string path, bool permanent = false)
        {
            return await TryExecuteIOAsync(async () =>
            {
                if (!Directory.Exists(path))
                    throw new DirectoryNotFoundException("Folder not found.");
                if (!OperatingSystem.IsWindows())
                {
                    // Fallback for Docker/Linux: always permanent delete
                    File.Delete(path);
                }
                else
                {
                    if (permanent)
                    {
                        // Permanent delete
                        Directory.Delete(path, true);
                    }
                    else
                    {
                        // Move folder to Recycle Bin (Windows only)
                        if (OperatingSystem.IsWindows())
                        {
                            VBIO.FileSystem.DeleteDirectory(path,
                            VBIO.UIOption.OnlyErrorDialogs,
                            VBIO.RecycleOption.SendToRecycleBin);
                        }
                    }
                }
                await Task.CompletedTask;
            }, permanent ? "Folder permanently deleted." : "Folder moved to Recycle Bin successfully."); 
        }

        public async Task<OperationResultDto> MoveFileAsync(string source, string destination)
        {
            return await TryExecuteIOAsync(async () =>
            {
                if (!File.Exists(source))
                    throw new FileNotFoundException("Source file not found.", source);

                var destPath = Path.Combine(destination, Path.GetFileName(source));
                if (File.Exists(destPath))
                    File.Delete(destPath);

                File.Move(source, destPath);
                await Task.CompletedTask;
            }, "File moved successfully.");
        }

        public async Task<OperationResultDto> MoveFolderAsync(string source, string destination)
        {
            return await TryExecuteIOAsync(async () =>
            {
                if (!Directory.Exists(source))
                    throw new DirectoryNotFoundException("Source folder not found.");

                var destPath = Path.Combine(destination, Path.GetFileName(source));
                if (Directory.Exists(destPath))
                    Directory.Delete(destPath, true);

                Directory.Move(source, destPath);
                await Task.CompletedTask;
            }, "Folder moved successfully.");
        }
        #endregion
        #region private
        private static async Task<long> GetDirectorySizeAsync(DirectoryInfo dir, bool recursive = true)
        {
            return await Task.Run(() =>
            {
                try
                {
                    long totalSize = 0;
                    var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                    foreach (var file in dir.EnumerateFiles("*", option))
                    {
                        try
                        {
                            totalSize += file.Length;
                        }
                        catch (UnauthorizedAccessException)
                        {
                            // Skip restricted files but continue scanning
                            continue;
                        }
                        catch (IOException)
                        {
                            // Skip files that can't be read
                            continue;
                        }
                    }

                    return totalSize;
                }
                catch (UnauthorizedAccessException)
                {
                    // Skip restricted folders completely
                    return 0;
                }
                catch (IOException)
                {
                    return 0;
                }
            });
        }
        //private async Task<List<FileInfoDto>> EnumerateFilesAsync(string path, bool includeHash = false, bool deepScan = false)
        //{
        //    var result = new List<FileInfoDto>();

        //    if (string.IsNullOrWhiteSpace(path))
        //        throw new ArgumentException("Path cannot be null or empty.");

        //    if (!Directory.Exists(path))
        //        throw new DirectoryNotFoundException($"Directory not found: {path}");

        //    var option = deepScan ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        //    var files = Directory.EnumerateFiles(path, "*", option);

        //    foreach (var file in files)
        //    {
        //        try
        //        {
        //            var info = new FileInfo(file);
        //            var dto = new FileInfoDto
        //            {
        //                Name = info.Name,
        //                SizeMB = Math.Round(info.Length / (1024.0 * 1024), 2),
        //                ModifiedDate = info.LastWriteTime
        //            };

        //            if (includeHash)
        //                dto.Hash = await ComputePartialHashAsync(file);

        //            result.Add(dto);
        //        }
        //        catch (UnauthorizedAccessException)
        //        {
        //            result.Add(new FileInfoDto
        //            {
        //                Name = Path.GetFileName(file),
        //                SizeMB = 0,
        //                ModifiedDate = DateTime.MinValue,
        //                Hash = "AccessDenied"
        //            });
        //        }
        //        catch (IOException ex)
        //        {
        //            result.Add(new FileInfoDto
        //            {
        //                Name = Path.GetFileName(file),
        //                SizeMB = 0,
        //                ModifiedDate = DateTime.MinValue,
        //                Hash = $"Error: {ex.Message}"
        //            });
        //        }
        //    }

        //    return result;
        //}
        private async Task<List<FileInfoDto>> EnumerateFilesAsync(string path, bool includeHash = false, bool deepScan = false)
        {
            var result = new List<FileInfoDto>();

            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be null or empty.");

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"Directory not found: {path}");

            // Include this folder and all subfolders if deepScan = true
            var folderQueue = new Queue<string>();
            folderQueue.Enqueue(path);

            while (folderQueue.Count > 0)
            {
                string currentFolder = folderQueue.Dequeue();

                // Add subfolders to queue
                try
                {
                    if (deepScan)
                    {
                        foreach (var subfolder in Directory.GetDirectories(currentFolder))
                            folderQueue.Enqueue(subfolder);
                    }
                }
                catch
                {
                    // skip inaccessible subfolder
                }

                // Process all files in the current folder
                string[] filesInFolder;
                try
                {
                    filesInFolder = Directory.GetFiles(currentFolder);
                }
                catch
                {
                    continue; // skip folder if inaccessible
                }

                foreach (var file in filesInFolder)
                {
                    try
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

                        result.Add(dto);
                    }
                    catch
                    {
                        result.Add(new FileInfoDto
                        {
                            Name = Path.GetFileName(file),
                            SizeMB = 0,
                            ModifiedDate = DateTime.MinValue,
                            Hash = "AccessDenied"
                        });
                    }
                }
            }

            return result;
        }
        public async Task<AIAnalysisResultDto> AnalyzeFilesInBatchesAsync(IEnumerable<FileInfoDto> files, int batchSize = 100)
        {
            var fileList = files.ToList();

            if (!fileList.Any())
                return new AIAnalysisResultDto
                {
                    Suggestions = new List<string> { "No files to analyze." }
                };

            var finalResult = new AIAnalysisResultDto
            {
                LargeFiles = new List<string>(),
                DuplicateFiles = new List<string>(),
                OldFiles = new List<string>(),
                Suggestions = new List<string>()
            };

            // Split files into batches
            var batches = fileList
                .Select((file, index) => new { file, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.file).ToList())
                .ToList();

            foreach (var batch in batches)
            {
                try
                {
                    // Call AI service for this batch
                    var batchResult = await _aiService.AnalyzeFilesAsync(batch);

                    if (batchResult != null)
                    {
                        finalResult.LargeFiles?.AddRange(batchResult.LargeFiles ?? []);
                        finalResult.DuplicateFiles?.AddRange(batchResult.DuplicateFiles ?? []);
                        finalResult.OldFiles?.AddRange(batchResult.OldFiles ?? []);
                        finalResult.Suggestions?.AddRange(batchResult.Suggestions ?? []);
                    }
                }
                catch (Exception ex)
                {
                    finalResult.Suggestions?.Add($"Batch error: {ex.Message}");
                }
            }

            // Deduplicate results
            finalResult.LargeFiles = finalResult.LargeFiles?.Distinct().ToList();
            finalResult.DuplicateFiles = finalResult.DuplicateFiles?.Distinct().ToList();
            finalResult.OldFiles = finalResult.OldFiles?.Distinct().ToList();
            finalResult.Suggestions = finalResult.Suggestions?.Distinct().ToList();

            return finalResult;
        }
        private static async Task<string> ComputePartialHashAsync(string filePath)
        {
            byte[] buffer = new byte[1024 * 256];
            using var md5 = MD5.Create();
            await using var stream = File.OpenRead(filePath);

            int bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length));
            byte[] hash = md5.ComputeHash(buffer, 0, bytesRead);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
        private async Task<OperationResultDto> TryExecuteIOAsync(Func<Task> operation, string successMessage)
        {
            try
            {
                await operation();
                return new OperationResultDto { Success = true, Message = successMessage };
            }
            catch (UnauthorizedAccessException)
            {
                return new OperationResultDto { Success = false, Message = "Access denied. Check permissions." };
            }
            catch (IOException ex)
            {
                return new OperationResultDto { Success = false, Message = $"I/O error: {ex.Message}" };
            }
            catch (Exception ex)
            {
                return new OperationResultDto { Success = false, Message = $"Unexpected error: {ex.Message}" };
            }
        }
        private static async Task<long> GetDirectorySizeParallelAsync(DirectoryInfo dir, bool recursive = true)
        {
            long totalSize = 0;
            var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            try
            {
                await Parallel.ForEachAsync(dir.EnumerateFiles("*", option), async (file, token) =>
                {
                    try
                    {
                        Interlocked.Add(ref totalSize, file.Length);
                    }
                    catch
                    {
                        // Skip unreadable files but continue
                    }

                    await Task.Yield(); // Let scheduler breathe
                });
            }
            catch (UnauthorizedAccessException)
            {
                // Skip restricted folders
            }
            catch (IOException)
            {
                // Skip I/O errors
            }

            return totalSize;
        }
        private static async Task<long> GetDirectorySizeSmartAsync(DirectoryInfo dir, bool recurse = true)
        {
            var fileCount = dir.EnumerateFiles("*", SearchOption.AllDirectories).Count();
            if (fileCount > 1000)
                return await GetDirectorySizeParallelAsync(dir, recursive: recurse);
            else
                return await GetDirectorySizeAsync(dir, recursive: recurse);
        }

        #endregion
    }
}
