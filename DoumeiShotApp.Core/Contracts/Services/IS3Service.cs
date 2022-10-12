namespace DoumeiShotApp.Core.Contracts.Services;

public interface IS3Service
{
    void ListBucket();

    string Upload(string folderPath);

    Task<bool> CheckFileExists(string fileName);

    string GetPreSignedURLFromFolderPath(string folderPath);

    void Delete(string filePath);
}
