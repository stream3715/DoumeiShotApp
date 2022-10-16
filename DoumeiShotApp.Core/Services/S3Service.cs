using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using DoumeiShotApp.Core.Contracts.Services;

namespace DoumeiShotApp.Core.Services;
public class S3Service : IS3Service
{
    private static readonly string BUCKET_NAME = "moa-doumeishot";
    private IAmazonS3 _mClient;

    public S3Service()
    {
        Initialized();
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public void Initialized()
    {
        AmazonS3Config config = new AmazonS3Config();
        config.RegionEndpoint = RegionEndpoint.APNortheast1;

        SharedCredentialsFile credentialsFile = new SharedCredentialsFile();
        CredentialProfile profile;

        if (credentialsFile.TryGetProfile("doumeishot", out profile) == false)
        {
            Console.WriteLine("プロファイルの取得に失敗しました。");
            return;
        }

        AWSCredentials awsCredentials;
        if (AWSCredentialsFactory.TryGetAWSCredentials(profile, credentialsFile, out awsCredentials) == false)
        {
            Console.WriteLine("認証情報の生成に失敗しました。");
        }

        _mClient = new AmazonS3Client(awsCredentials, config);
    }

    public void ListBucket()
    {
        var response = GetS3BucketsList();
        if (response.Result != null)
        {
            foreach (var bucket in response.Result.Buckets)
            {
                Console.WriteLine($"BucketName : {bucket.BucketName}");
            }
        }
        return;
    }

    private async Task<ListBucketsResponse> GetS3BucketsList()
    {
        try
        {
            var res = await _mClient.ListBucketsAsync();
            return res;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        return null;
    }

    public (string, DateTime) Upload(string folderPath)
    {
        TransferUtility fileTransferUtility = new(_mClient);
        fileTransferUtility.UploadAsync(folderPath, BUCKET_NAME).Wait();
        return GetPreSignedURLFromFolderPath(folderPath);
    }

    public async Task<bool> CheckFileExists(string fileName)
    {
        var response = await GetS3ObjectMetadata(fileName);
        if (response != null)
        {
            return true;
        }
        return false;
    }

    private async Task<GetObjectMetadataResponse> GetS3ObjectMetadata(string fileName)
    {
        try
        {
            var res = await _mClient.GetObjectMetadataAsync(new GetObjectMetadataRequest()
            {
                BucketName = BUCKET_NAME,
                Key = fileName
            });
            return res;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        return null;
    }

    public (string, DateTime) GetPreSignedURLFromFolderPath(string folderPath)
    {
        var expires = DateTime.Now.AddHours(24);
        return (GetPreSignedURL(Path.GetFileName(folderPath), expires), expires);
    }

    private string GetPreSignedURL(string fileName, DateTime expiresDateTime)
    {
        var req = new GetPreSignedUrlRequest()
        {
            BucketName = BUCKET_NAME,
            Key = fileName,
            Expires = expiresDateTime,
        };

        var res = _mClient.GetPreSignedURL(req);
        return res;
    }

    public void Delete(string filePath) => throw new NotImplementedException();
}
