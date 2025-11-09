using AIFileManager.DTO;

namespace AIFileManager.Interfaces
{
    public interface IFileStorageService
    {
        Task<IEnumerable<DriveInfoDto>> GetDrivesAsync();
        Task<IEnumerable<FolderInfoDto>> GetFoldersAsync(string path);
        Task<IEnumerable<FileInfoDto>> GetFilesAsync(string path);
        Task<bool> DeleteFileAsync(string path);
        Task<bool> DeleteFolderAsync(string path);
        Task<bool> MoveFileAsync(string source, string destination);
        Task<bool> MoveFolderAsync(string source, string destination);
        Task<IEnumerable<FileInfoDto>> GetFileMetadataAsync(string path);
    }
}
