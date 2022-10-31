namespace DoumeiShotApp.Core.Contracts.Services;

public interface IS3Service
{
    void ListBucket();

    (string, DateTime) Upload(string folderPath);

    Task<bool> CheckFileExists(string fileName);

    (string, DateTime) GetPreSignedURLFromFolderPath(string folderPath);

    void Delete(string filePath);

    Task<bool> WipeBucket();
}
