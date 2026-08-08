using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GachaGame.Data;
using UnityEngine;

namespace GachaGame.Core
{
    // AES-256-CBC encrypts PlayerSaveData at rest (random IV per save, prepended to the
    // ciphertext) and wraps it in an HMAC-checked envelope, so a manually edited save file is
    // both unreadable and detected as tampered on load.
    public class SaveSystem
    {
        private readonly byte[] encryptionKey; // 32 bytes -> AES-256
        private readonly byte[] macKey;

        public SaveSystem(string passphrase)
        {
            if (string.IsNullOrEmpty(passphrase))
                throw new ArgumentException("Passphrase must not be empty.", nameof(passphrase));

            using var sha256 = SHA256.Create();
            encryptionKey = sha256.ComputeHash(Encoding.UTF8.GetBytes(passphrase + ":enc"));
            macKey = sha256.ComputeHash(Encoding.UTF8.GetBytes(passphrase + ":mac"));
        }

        public async Task SaveGameAsync(PlayerSaveData data, string filePath)
        {
            string payload = JsonUtility.ToJson(data);
            string hmac = SaveDataValidator.ComputeHmac(payload, macKey);
            string envelopeJson = JsonUtility.ToJson(new SaveEnvelope { payload = payload, hmac = hmac });

            byte[] encrypted = Encrypt(envelopeJson);
            await File.WriteAllBytesAsync(filePath, encrypted);
        }

        public async Task<PlayerSaveData> LoadGameAsync(string filePath)
        {
            if (!File.Exists(filePath)) return null;

            byte[] encrypted = await File.ReadAllBytesAsync(filePath);
            string envelopeJson = Decrypt(encrypted);
            var envelope = JsonUtility.FromJson<SaveEnvelope>(envelopeJson);

            if (!SaveDataValidator.Validate(envelope.payload, envelope.hmac, macKey))
                throw new InvalidDataException("Save data failed integrity validation and may have been tampered with.");

            return JsonUtility.FromJson<PlayerSaveData>(envelope.payload);
        }

        public byte[] Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Key = encryptionKey;
            aes.GenerateIV();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            byte[] result = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);
            return result;
        }

        public string Decrypt(byte[] data)
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Key = encryptionKey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            int ivLength = aes.BlockSize / 8;
            byte[] iv = new byte[ivLength];
            byte[] cipherBytes = new byte[data.Length - ivLength];
            Buffer.BlockCopy(data, 0, iv, 0, ivLength);
            Buffer.BlockCopy(data, ivLength, cipherBytes, 0, cipherBytes.Length);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
