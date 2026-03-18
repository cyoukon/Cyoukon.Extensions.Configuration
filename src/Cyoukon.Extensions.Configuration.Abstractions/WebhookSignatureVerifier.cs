using System;
using System.Security.Cryptography;
using System.Text;

namespace Cyoukon.Extensions.Configuration.Abstractions
{
    public static class WebhookSignatureVerifier
    {
        public static bool VerifyHmacSha256Signature(string timestamp, string signature, string secret)
        {
            try
            {
                var stringToSign = $"{timestamp}\n{secret}";
                var secretBytes = Encoding.UTF8.GetBytes(secret);
                var stringToSignBytes = Encoding.UTF8.GetBytes(stringToSign);

                using var hmac = new HMACSHA256(secretBytes);
                var hashBytes = hmac.ComputeHash(stringToSignBytes);
                var computedSignature = Convert.ToBase64String(hashBytes);

                return string.Equals(signature, computedSignature, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public static string ComputeHmacSha256Signature(string timestamp, string secret)
        {
            var stringToSign = $"{timestamp}\n{secret}";
            var secretBytes = Encoding.UTF8.GetBytes(secret);
            var stringToSignBytes = Encoding.UTF8.GetBytes(stringToSign);

            using var hmac = new HMACSHA256(secretBytes);
            var hashBytes = hmac.ComputeHash(stringToSignBytes);
            return Convert.ToBase64String(hashBytes);
        }

        public static bool VerifySha256Signature(string payload, string signature, string secret)
        {
            try
            {
                var expectedPrefix = "sha256=";
                if (!signature.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                var signatureHash = signature.Substring(expectedPrefix.Length);
                var secretBytes = Encoding.UTF8.GetBytes(secret);
                var payloadBytes = Encoding.UTF8.GetBytes(payload);

                using var hmac = new HMACSHA256(secretBytes);
                var hashBytes = hmac.ComputeHash(payloadBytes);
                var computedSignature = BytesToHexString(hashBytes).ToLowerInvariant();

                return string.Equals(signatureHash, computedSignature, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public static string ComputeSha256Signature(string payload, string secret)
        {
            var secretBytes = Encoding.UTF8.GetBytes(secret);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            using var hmac = new HMACSHA256(secretBytes);
            var hashBytes = hmac.ComputeHash(payloadBytes);
            return "sha256=" + BytesToHexString(hashBytes).ToLowerInvariant();
        }

        private static string BytesToHexString(byte[] bytes)
        {
            var hex = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }
    }
}
