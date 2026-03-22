using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TripMeta.Enterprise.Services
{
    /// <summary>
    /// 安全服务
    /// 处理2FA验证、IP白名单、数据加密
    /// </summary>
    public class SecurityService
    {
        private readonly HashSet<string> allowedIPs = new HashSet<string>();
        private readonly Dictionary<string, TwoFASetup> twoFASetups = new Dictionary<string, TwoFASetup>();
        private readonly bool ipWhitelistEnabled;
        private readonly bool encryptionEnabled;

        // AES密钥（实际项目中应从安全密钥管理服务获取）
        private readonly byte[] encryptionKey;
        private readonly byte[] encryptionIV;

        public SecurityService(bool enableIPWhitelist = true, bool enableEncryption = true)
        {
            this.ipWhitelistEnabled = enableIPWhitelist;
            this.encryptionEnabled = enableEncryption;

            // 初始化加密密钥（固定用于演示，生产中使用密钥管理服务）
            using (var aes = Aes.Create())
            {
                aes.GenerateKey();
                aes.GenerateIV();
                encryptionKey = aes.Key;
                encryptionIV = aes.IV;
            }

            // 默认允许本地回环地址
            allowedIPs.Add("127.0.0.1");
            allowedIPs.Add("::1");
            allowedIPs.Add("0.0.0.0");
        }

        #region 2FA

        /// <summary>
        /// 设置2FA
        /// </summary>
        public TwoFASetupResult Setup2FA(string userId)
        {
            var secret = GenerateTOTPSecret();
            var setup = new TwoFASetup
            {
                userId = userId,
                secret = secret,
                isEnabled = false,
                createdAt = DateTime.Now
            };
            twoFASetups[userId] = setup;

            return new TwoFASetupResult
            {
                secret = secret,
                qrCodeUrl = $"otpauth://totp/TripMeta:{userId}?secret={secret}&issuer=TripMeta"
            };
        }

        /// <summary>
        /// 验证2FA代码
        /// </summary>
        public async Task<bool> Verify2FA(string userId, string code)
        {
            await Task.Delay(20);

            if (!twoFASetups.TryGetValue(userId, out var setup))
            {
                // 用户未设置2FA，允许通过（或根据策略拒绝）
                return true;
            }

            // 验证TOTP代码（6位数字）
            bool isValid = ValidateTOTP(setup.secret, code);

            if (isValid && !setup.isEnabled)
            {
                setup.isEnabled = true;
            }

            return isValid;
        }

        private bool ValidateTOTP(string secret, string code)
        {
            if (string.IsNullOrEmpty(code) || code.Length != 6) return false;
            if (!int.TryParse(code, out _)) return false;

            // 模拟TOTP验证（实际使用Google Authenticator兼容算法）
            long timeStep = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
            for (long i = -1; i <= 1; i++)
            {
                string expectedCode = GenerateTOTPCode(secret, timeStep + i);
                if (expectedCode == code) return true;
            }
            return false;
        }

        private string GenerateTOTPCode(string secret, long timeStep)
        {
            byte[] secretBytes = Encoding.UTF8.GetBytes(secret);
            byte[] timeBytes = BitConverter.GetBytes(timeStep);
            if (BitConverter.IsLittleEndian) Array.Reverse(timeBytes);

            using (var hmac = new HMACSHA1(secretBytes))
            {
                byte[] hash = hmac.ComputeHash(timeBytes);
                int offset = hash[hash.Length - 1] & 0x0F;
                int code = ((hash[offset] & 0x7F) << 24) |
                           ((hash[offset + 1] & 0xFF) << 16) |
                           ((hash[offset + 2] & 0xFF) << 8) |
                           (hash[offset + 3] & 0xFF);
                return (code % 1000000).ToString("D6");
            }
        }

        private string GenerateTOTPSecret()
        {
            byte[] bytes = new byte[20];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes).Replace("=", "").Replace("+", "A").Replace("/", "B").Substring(0, 16);
        }

        #endregion

        #region IP白名单

        /// <summary>
        /// 检查IP是否在白名单中
        /// </summary>
        public bool IsIPAllowed(string ipAddress)
        {
            if (!ipWhitelistEnabled) return true;
            if (string.IsNullOrEmpty(ipAddress)) return false;
            return allowedIPs.Contains(ipAddress);
        }

        /// <summary>
        /// 添加IP到白名单
        /// </summary>
        public void AddAllowedIP(string ipAddress)
        {
            if (!string.IsNullOrEmpty(ipAddress))
            {
                allowedIPs.Add(ipAddress);
                Debug.Log($"[SecurityService] Added IP to whitelist: {ipAddress}");
            }
        }

        /// <summary>
        /// 从白名单移除IP
        /// </summary>
        public bool RemoveAllowedIP(string ipAddress)
        {
            return allowedIPs.Remove(ipAddress);
        }

        /// <summary>
        /// 获取白名单列表
        /// </summary>
        public IReadOnlyCollection<string> GetAllowedIPs() => allowedIPs;

        #endregion

        #region 数据加密

        /// <summary>
        /// 加密数据（AES-256）
        /// </summary>
        public string Encrypt(string plainText)
        {
            if (!encryptionEnabled || string.IsNullOrEmpty(plainText)) return plainText;

            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = encryptionKey;
                    aes.IV = encryptionIV;

                    var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                    var plainBytes = Encoding.UTF8.GetBytes(plainText);
                    var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                    return Convert.ToBase64String(cipherBytes);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SecurityService] Encryption failed: {ex.Message}");
                return plainText;
            }
        }

        /// <summary>
        /// 解密数据
        /// </summary>
        public string Decrypt(string cipherText)
        {
            if (!encryptionEnabled || string.IsNullOrEmpty(cipherText)) return cipherText;

            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = encryptionKey;
                    aes.IV = encryptionIV;

                    var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    var cipherBytes = Convert.FromBase64String(cipherText);
                    var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                    return Encoding.UTF8.GetString(plainBytes);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SecurityService] Decryption failed: {ex.Message}");
                return cipherText;
            }
        }

        /// <summary>
        /// 计算数据哈希（SHA-256）
        /// </summary>
        public string ComputeHash(string data)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            }
        }

        #endregion
    }

    /// <summary>
    /// 2FA设置信息
    /// </summary>
    public class TwoFASetup
    {
        public string userId;
        public string secret;
        public bool isEnabled;
        public DateTime createdAt;
    }

    /// <summary>
    /// 2FA设置结果
    /// </summary>
    public class TwoFASetupResult
    {
        public string secret;
        public string qrCodeUrl;
    }
}
