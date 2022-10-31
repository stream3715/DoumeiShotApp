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
    public async Task<bool> WipeBucket()
    {
        var fileListResult = await ListBucketContentsAsync();
        if (fileListResult == null) return false;

        // 並列実行オプション
        var options = new ParallelOptions();
        options.MaxDegreeOfParallelism = 100; // 最大実行数
        var keyVersionToDelete = new List<KeyVersion>();

        Parallel.ForEach(fileListResult, options, s3Object =>
        {
            keyVersionToDelete.Add(new KeyVersion() { Key = s3Object.Key });
        });

        var result = await _mClient.DeleteObjectsAsync(new DeleteObjectsRequest
        {
            BucketName = BUCKET_NAME,
            Objects = keyVersionToDelete
        });

        return result.HttpStatusCode < System.Net.HttpStatusCode.Ambiguous;
    }

    private async Task<List<S3Object>> ListBucketContentsAsync()
    {
        var fileList = new List<S3Object>();
        try
        {
            var request = new ListObjectsV2Request
            {
                BucketName = BUCKET_NAME,
            };

            var response = new ListObjectsV2Response();

            do
            {
                response = await _mClient.ListObjectsV2Async(request);

                response.S3Objects.ForEach(obj => fileList.Add(obj));

                // If the response is truncated, set the request ContinuationToken
                // from the NextContinuationToken property of the response.
                request.ContinuationToken = response.NextContinuationToken;
            }
            while (response.IsTruncated);

            return fileList;
        }
        catch (AmazonS3Exception ex)
        {
            Console.WriteLine($"Error encountered on server. Message:'{ex.Message}' getting list of objects.");
            return null;
        }
    }

}
