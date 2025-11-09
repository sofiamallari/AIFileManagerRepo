using AIFileManager.DTO;
using AIFileManager.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace AIFileManager.Services
{
    public class FileStorageService : IFileStorageService
    {
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

        public async Task<IEnumerable<FolderInfoDto>> GetFoldersAsync(string path)
        {
            return await Task.Run(() =>
            {
                return Directory.GetDirectories(path).Select(folder =>
                {
                    var dirInfo = new DirectoryInfo(folder);
                    return new FolderInfoDto
                    {
                        Name = dirInfo.Name,
                        SizeMB = Math.Round(GetDirectorySize(dirInfo) / (1024.0 * 1024), 2),
                        LastModified = dirInfo.LastWriteTime
                    };
                }).ToList();
            });
        }

        private static long GetDirectorySize(DirectoryInfo dir)
        {
            return dir.EnumerateFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);
        }

        public async Task<IEnumerable<FileInfoDto>> GetFilesAsync(string path)
        {
            return await Task.Run(() =>
            {
                return Directory.GetFiles(path).Select(file =>
                {
                    var info = new FileInfo(file);
                    return new FileInfoDto
                    {
                        Name = info.Name,
                        SizeMB = Math.Round(info.Length / (1024.0 * 1024), 2),
                        ModifiedDate = info.LastWriteTime
                    };
                }).ToList();
            });
        }

        public async Task<bool> DeleteFileAsync(string path)
        {
            return await Task.Run(() =>
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    return true;
                }
                return false;
            });
        }

        public async Task<bool> DeleteFolderAsync(string path)
        {
            return await Task.Run(() =>
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                    return true;
                }
                return false;
            });
        }
        public async Task<bool> MoveFileAsync(string source, string destination)
        {
            return await Task.Run(() =>
            {
                if (!File.Exists(source))
                    return false;

                try
                {
                    var destPath = Path.Combine(destination, Path.GetFileName(source));

                    // If destination already exists, delete first
                    if (File.Exists(destPath))
                        File.Delete(destPath);

                    File.Move(source, destPath);
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        public async Task<bool> MoveFolderAsync(string source, string destination)
        {
            return await Task.Run(() =>
            {
                if (!Directory.Exists(source))
                    return false;

                try
                {
                    var destPath = Path.Combine(destination, Path.GetFileName(source));

                    // If destination folder exists, delete before moving
                    if (Directory.Exists(destPath))
                        Directory.Delete(destPath, true);

                    Directory.Move(source, destPath);
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }


        public async Task<IEnumerable<FileInfoDto>> GetFileMetadataAsync(string path)
        {
            return await Task.Run(() =>
            {
                return Directory.GetFiles(path).Select(file =>
                {
                    var info = new FileInfo(file);
                    return new FileInfoDto
                    {
                        Name = info.Name,
                        SizeMB = Math.Round(info.Length / (1024.0 * 1024), 2),
                        ModifiedDate = info.LastWriteTime,
                        Hash = ComputePartialHash(file)
                    };
                }).ToList();
            });
        }

        private static string ComputePartialHash(string filePath)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filePath);
            byte[] buffer = new byte[1024 * 256];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            byte[] hash = md5.ComputeHash(buffer, 0, bytesRead);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
