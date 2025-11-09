using AIFileManager.DTO;

namespace AIFileManager.Interfaces
{
    public interface IFileStorageService
    {
        Task<IEnumerable<DriveInfoDto>> GetDrivesAsync();
        Task<IEnumerable<FolderInfoDto>> GetFoldersAsync(string path, bool deepScan = false);
        Task<IEnumerable<FileInfoDto>> GetFilesAsync(string path, bool deepScan = false);
        Task<OperationResultDto> DeleteFileAsync(string path, bool permanent = false);
        Task<OperationResultDto> DeleteFolderAsync(string path, bool permanent = false);
        Task<OperationResultDto> MoveFileAsync(string source, string destination);
        Task<OperationResultDto> MoveFolderAsync(string source, string destination);
        Task<IEnumerable<FileInfoDto>> GetFileMetadataAsync(string path, bool deepScan = false);
    }
}
