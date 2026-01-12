using InterviewService.Api.Helpers.EncryptionHelpers.Models;

using Microsoft.Extensions.Options;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Security.Cryptography;
using System.Text;

namespace InterviewService.Api.Helpers.EncryptionHelpers.Handlers
{
    public class EncryptionHelper(IOptions<EncryptionSecretKey> secretKey)
    {
        private readonly string _key = secretKey.Value.Secret;

        public string PadBase64(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: return base64 + "==";
                case 3: return base64 + "=";
                default: return base64;
            }
        }

        public string? DecryptFromReact(string base64Input)
        {

            byte[] keyBytes = ConvertHexStringToBytes(_key);
            string json = Encoding.UTF8.GetString(Convert.FromBase64String(base64Input));
            JObject? payload = JsonConvert.DeserializeObject<JObject>(json);

            if (payload == null || payload["iv"] == null || payload["value"] == null)
            {
                return null;
            }

            byte[] iv = Convert.FromBase64String((string)payload["iv"]!);
            byte[] cipherText = Convert.FromBase64String((string)payload["value"]!);
            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var decryptor = aes.CreateDecryptor();
                using var ms = new MemoryStream(cipherText);
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr = new StreamReader(cs);

                return sr.ReadToEnd();
            }
        }

        // Helper: Convert hex string to byte[]
        private static byte[] ConvertHexStringToBytes(string hex)
        {
            byte[] bytes = new byte[hex.Length / 2];

            for (int i = 0; i < hex.Length; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

            return bytes;
        }

        public string EncryptForReact(string plainText)
        {
            byte[] keyBytes = ConvertHexStringToBytes(_key);
            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.GenerateIV();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            using var sw = new StreamWriter(cs);
            sw.Write(plainText);
            sw.Close();

            string value = Convert.ToBase64String(ms.ToArray());
            string iv = Convert.ToBase64String(aes.IV);

            var result = new
            {
                iv,
                value
            };

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result)));
        }
    }
}
