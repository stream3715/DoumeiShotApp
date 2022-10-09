using System.Text;
using Amazon.Runtime.CredentialManagement;
using Amazon.Runtime;
using Amazon.S3;
using DoumeiShotApp.Core.Contracts.Services;
using Newtonsoft.Json;
using Amazon;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace DoumeiShotApp.Core.Services;
public class S3Service : IS3Service
{
    private static string BUCKET_NAME = "moa-doumeishot";
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
            foreach(var bucket in response.Result.Buckets)
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

    public string Upload(string folderPath)
    {
        TransferUtility fileTransferUtility = new(_mClient);
        fileTransferUtility.UploadAsync(folderPath, BUCKET_NAME).Wait();
        var preSignedUrl = _mClient.GetPreSignedURL(new GetPreSignedUrlRequest()
        {
            BucketName = BUCKET_NAME,
            Key = Path.GetFileName(folderPath),
            Expires = DateTime.Now.AddHours(24)
        });
        return preSignedUrl;
    }

    public void Delete(string filePath) => throw new NotImplementedException();
}
