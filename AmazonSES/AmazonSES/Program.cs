using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace AmazonSES
{
    class Program
    {
        private static IAmazonS3 s3Client;
        private static readonly RegionEndpoint defaultRegion = RegionEndpoint.USEast1;
        private static readonly BasicAWSCredentials awsCredentials = new BasicAWSCredentials("yourAccessKey", "yourSecretKey");
        private static readonly string bucketName = "bucketName";
        private static readonly string queueName = "qeueName";

        static void Main(string[] args)
        {
            MailDataModel mailDataModel = new MailDataModel();
            mailDataModel.FromEmail = "fromEmail";
            mailDataModel.ToEmail = "toEmail";
            mailDataModel.FullName = "Receiver Full Name";
            mailDataModel.Title = "Title";


            s3Client = new AmazonS3Client(awsCredentials, defaultRegion);

            string bucketKey = Guid.NewGuid().ToString();

            AddBucket(bucketName, bucketKey, JsonConvert.SerializeObject(mailDataModel)).Wait();

            SendToSqs(bucketKey, queueName).Wait();
        }

        public static async Task AddBucket(string bucketName, string keyName, string body)
        {
            try
            {
                PutObjectRequest request = new PutObjectRequest();
                request.BucketName = bucketName;
                request.Key = string.Concat(keyName, ".json");
                request.ContentType = "application/json";
                request.ContentBody = body;
                request.CannedACL = S3CannedACL.Private;

                await s3Client.PutObjectAsync(request);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }

        }

        public static async Task<bool> SendToSqs(string messageBody, string queueName)
        {
            try
            {
                IAmazonSQS amazonSQS = new AmazonSQSClient(awsCredentials, defaultRegion);
                var sqsMessageRequest = new SendMessageRequest
                {
                    QueueUrl = amazonSQS.GetQueueUrlAsync(queueName).Result.QueueUrl,
                    MessageBody = messageBody
                };
                await amazonSQS.SendMessageAsync(sqsMessageRequest);
                return true;
            }
            catch (Exception exception)
            {
                Console.WriteLine("Excetpion: {0}", exception.Message);
                return false;
            }
        }
    }
}
