using System;
using System.Security.Cryptography;
using System.Text;

namespace GachaGame.Core
{
    // HMAC-SHA256 integrity check over the save payload, independent of the AES encryption
    // SaveSystem applies for confidentiality - catches manual edits to a decrypted save file.
    public static class SaveDataValidator
    {
        public static string ComputeHmac(string payload, byte[] key)
        {
            using var hmac = new HMACSHA256(key);
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload ?? string.Empty));
            return Convert.ToBase64String(hash);
        }

        public static bool Validate(string payload, string expectedHmac, byte[] key)
        {
            if (string.IsNullOrEmpty(expectedHmac)) return false;

            try
            {
                byte[] actualBytes = Convert.FromBase64String(ComputeHmac(payload, key));
                byte[] expectedBytes = Convert.FromBase64String(expectedHmac);
                return CryptographicOperations.FixedTimeEquals(actualBytes, expectedBytes);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
