using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AutoUpdaterDotNET;

namespace AutoUpdaterTestWPF
{
    public class AzureStorageAuthentication : IAuthentication
    {
        private string StorageKey { get; }
        private string StorageAccount { get; }
        private string Method { get; } = "GET";

        public AzureStorageAuthentication(string storageKey, string storageAccount)
        {
            this.StorageKey = storageKey;
            this.StorageAccount = storageAccount;
        }

        public void Apply(Uri requestUri, WebHeaderCollection webHeaderCollection)
        {
            // Init
            string now = DateTime.UtcNow.ToString("R");
            string absolutePath = requestUri.AbsolutePath;
            string containerName = absolutePath.Split('/')[1];
            string blobName = absolutePath.Remove(0, $"/{containerName}/".Length);

            // Adjust WebHeaderCollection
            webHeaderCollection["x-ms-date"] = now;
            webHeaderCollection[HttpRequestHeader.Authorization] =
                this.CreateAuthorizationHeader(
                    this.Method,
                    now,
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