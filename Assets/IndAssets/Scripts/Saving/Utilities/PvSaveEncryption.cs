using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace ProjectCI.CoreSystem.Runtime.Saving.Utilities
{
    /// <summary>
    /// Encryption utility for save file protection
    /// Uses AES-256 encryption with SHA256 checksum for integrity verification
    /// </summary>
    public static class PvSaveEncryption
    {
        // Encryption key (32 bytes for AES-256)
        // In production, consider using a more secure key management system
        private static readonly byte[] EncryptionKey = Encoding.UTF8.GetBytes("ProjectCI_SaveKey_32ByteLength!!");
        
        private const string ChecksumSeparator = "|CHECKSUM|";
        
        /// <summary>
        /// Encrypt save data JSON string
        /// </summary>
        /// <param name="plainText">Plain JSON text to encrypt</param>
        /// <returns>Encrypted data with checksum (format: checksum|CHECKSUM|encryptedBase64)</returns>
        public static string EncryptSaveData(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return string.Empty;
            }
            
            try
            {
                // Calculate checksum before encryption
                string checksum = CalculateChecksum(plainText);
                
                // Encrypt the data
                string encryptedData = EncryptAES(plainText);
                
                // Combine checksum and encrypted data
                return $"{checksum}{ChecksumSeparator}{encryptedData}";
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to encrypt save data: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Decrypt save data and verify integrity
        /// </summary>
        /// <param name="encryptedData">Encrypted data with checksum</param>
        /// <returns>Decrypted JSON string, or null if verification fails</returns>
        public static string DecryptSaveData(string encryptedData)
        {
            if (string.IsNullOrEmpty(encryptedData))
            {
                return null;
            }
            
            try
            {
                // Split checksum and encrypted data
                int separatorIndex = encryptedData.IndexOf(ChecksumSeparator, StringComparison.Ordinal);
                if (separatorIndex < 0)
                {
                    Debug.LogError("Invalid encrypted data format: checksum separator not found");
                    return null;
                }
                
                string checksum = encryptedData.Substring(0, separatorIndex);
                string encryptedBase64 = encryptedData.Substring(separatorIndex + ChecksumSeparator.Length);
                
                // Decrypt the data
                string decryptedData = DecryptAES(encryptedBase64);
                if (string.IsNullOrEmpty(decryptedData))
                {
                    Debug.LogError("Failed to decrypt save data");
                    return null;
                }
                
                // Verify checksum
                string calculatedChecksum = CalculateChecksum(decryptedData);
                if (calculatedChecksum != checksum)
                {
                    Debug.LogError("Save data integrity check failed: checksum mismatch. File may have been tampered with.");
                    return null;
                }
                
                return decryptedData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to decrypt save data: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Encrypt plain text using AES-256
        /// </summary>
        private static string EncryptAES(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = EncryptionKey;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                
                // Generate random IV for each encryption
                aes.GenerateIV();
                
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                
                using (MemoryStream ms = new MemoryStream())
                {
                    // Write IV first (16 bytes for AES)
                    ms.Write(aes.IV, 0, aes.IV.Length);
                    
                    // Then write encrypted data
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                    }
                    
                    // Convert to Base64 for storage
                    byte[] encryptedBytes = ms.ToArray();
                    return Convert.ToBase64String(encryptedBytes);
                }
            }
        }
        
        /// <summary>
        /// Decrypt encrypted data using AES-256
        /// </summary>
        private static string DecryptAES(string encryptedBase64)
        {
            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedBase64);
                
                using (Aes aes = Aes.Create())
                {
                    aes.Key = EncryptionKey;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    
                    // Read IV from first 16 bytes
                    byte[] iv = new byte[16];
                    Array.Copy(encryptedBytes, 0, iv, 0, 16);
                    aes.IV = iv;
                    
                    // Read encrypted data (skip IV)
                    int encryptedDataLength = encryptedBytes.Length - 16;
                    byte[] encryptedData = new byte[encryptedDataLength];
                    Array.Copy(encryptedBytes, 16, encryptedData, 0, encryptedDataLength);
                    
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    
                    using (MemoryStream ms = new MemoryStream(encryptedData))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader sr = new StreamReader(cs))
                            {
                                return sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"AES decryption failed: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Calculate SHA256 checksum for data integrity verification
        /// </summary>
        private static string CalculateChecksum(string data)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
