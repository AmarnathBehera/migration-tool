using BDO.GWT.Common.Options;
using Microsoft.Extensions.Options;
using System;
using System.Security.Cryptography;
using System.Text;

namespace BDO.GWT.Common.Helpers
{
    public interface ISecretsEncryptionHelper
    {
        string EncryptSecret(string secret);

        string DecryptSecret(string encryptedSecret);
    }

    public class SecretsEncryptionHelper : ISecretsEncryptionHelper
    {
        private readonly IOptions<SecretsEncryptionOptions> _secretsEncryptionOptions;

        public SecretsEncryptionHelper(IOptions<SecretsEncryptionOptions> secretsEncryptionOptions)
        {
            _secretsEncryptionOptions = secretsEncryptionOptions;
        }

        public string EncryptSecret(string secret)
        {
            using (var aes = ConfigureAesEncryption())
            {
                using (var encryptor = aes.CreateEncryptor())
                {
                    var plainTextBytes = Encoding.UTF8.GetBytes(secret);

                    var cipherTextBytes = encryptor.TransformFinalBlock(plainTextBytes, 0, plainTextBytes.Length);

                    var encryptedSecret = EncodeInitializationVectorAndCipherText(aes.IV, cipherTextBytes);

                    return encryptedSecret;
                }
            }
        }

        public string DecryptSecret(string encryptedSecret)
        {
            using (var aes = ConfigureAesEncryption())
            {
                var (initializationVectorBytes, cipherTextBytes) = DecodeInitializationVectorAndCipherText(encryptedSecret, aes.IV.Length);

                aes.IV = initializationVectorBytes;

                using (var decryptor = aes.CreateDecryptor())
                {
                    var plainTextBytes = decryptor.TransformFinalBlock(cipherTextBytes, 0, cipherTextBytes.Length);

                    var plainText = Encoding.UTF8.GetString(plainTextBytes);

                    return plainText;
                }
            }
        }

        private Aes ConfigureAesEncryption()
        {
            var aes = Aes.Create();

            aes.Mode = CipherMode.CBC;
            aes.Key = Convert.FromBase64String(_secretsEncryptionOptions.Value.SecretKey);
            aes.Padding = PaddingMode.PKCS7;

            return aes;
        }

        private static string EncodeInitializationVectorAndCipherText(byte[] initializationVectorBytes, byte[] cipherTextBytes)
        {
            var bytes = new byte[initializationVectorBytes.Length + cipherTextBytes.Length];

            Array.Copy(initializationVectorBytes, 0, bytes, 0, initializationVectorBytes.Length);

            Array.Copy(cipherTextBytes, 0, bytes, initializationVectorBytes.Length, cipherTextBytes.Length);

            var encryptedSecret = Convert.ToBase64String(bytes);

            return encryptedSecret;
        }

        private static (byte[] initializationVectorBytes, byte[] cipherTextBytes) DecodeInitializationVectorAndCipherText(string encryptedSecret, int initializationVectorLength)
        {
            var bytes = Convert.FromBase64String(encryptedSecret);

            var initializationVectorBytes = new byte[initializationVectorLength];

            Array.Copy(bytes, 0, initializationVectorBytes, 0, initializationVectorLength);

            var cipherTextBytes = new byte[bytes.Length - initializationVectorLength];

            Array.Copy(bytes, initializationVectorLength, cipherTextBytes, 0, bytes.Length - initializationVectorLength);

            return (initializationVectorBytes, cipherTextBytes);
        }
    }
}
