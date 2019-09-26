using System;
using System.Net;
using System.Text;
using System.Security.Cryptography;
using AutoUpdaterDotNET;

namespace AutoUpdaterTestWPF
{
    public class AzureStorageAuthentication : IAuthentication
    {
        private string StorageKey { get; }
        private string StorageAccount { get; }
        private string Method { get; } = "GET";
        private string Now { get; } = DateTime.UtcNow.ToString("R");

        public AzureStorageAuthentication(string storageKey, string storageAccount)
        {
            this.StorageKey = storageKey;
            this.StorageAccount = storageAccount;
        }

        public void Apply(Uri requestUri, WebHeaderCollection webHeaderCollection)
        {
            // Init
            string absolutePath = requestUri.AbsolutePath;
            string containerName = absolutePath.Split('/')[1];
            string blobName = absolutePath.Remove(0, $"/{containerName}/".Length);

            // Adjust WebHeaderCollection
            webHeaderCollection["x-ms-date"] = this.Now;
            webHeaderCollection[HttpRequestHeader.Authorization] =
                this.CreateAuthorizationHeader(
                    this.Method,
                    this.Now,
                    this.StorageAccount,
                    this.StorageKey,
                    containerName,
                    blobName);
        }

        private string CreateAuthorizationHeader(string method, string now, string storageAccount, string storageKey, string containerName, string blobName)
        {
            // Init
            string stringToSign = "GET\n\n\n\nx-ms-date:" + now + "\n/" + storageAccount + "/" + containerName + "/" + blobName;
            HMACSHA256 hmac = new HMACSHA256(Convert.FromBase64String(storageKey));
            string signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));

            // Create Authorization Header
            string authorizationHeader = string.Format("{0} {1}:{2}", "SharedKey", storageAccount, signature);

            return authorizationHeader;
        }
    }
}