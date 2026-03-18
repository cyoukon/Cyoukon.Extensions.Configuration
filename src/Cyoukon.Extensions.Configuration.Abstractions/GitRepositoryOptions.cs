using System;

namespace Cyoukon.Extensions.Configuration.Abstractions
{
    public abstract class GitRepositoryOptions
    {
        public string AccessToken { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public string Repo { get; set; } = string.Empty;
        public string Branch { get; set; } = "main";
        public string ConfigPath { get; set; } = "config";
        public string ConfigFileName { get; set; } = "appsettings.json";
        public bool EnableEncryption { get; set; }
        public string? EncryptionKey { get; set; }
        public bool EnableReload { get; set; }
        public TimeSpan ReloadInterval { get; set; } = TimeSpan.FromMinutes(5);
        public string? WebhookSecret { get; set; }

        public string GetFilePath()
        {
            return $"{ConfigPath}/{ConfigFileName}".TrimStart('/');
        }

        public virtual void Validate()
        {
            if (string.IsNullOrWhiteSpace(AccessToken))
            {
                throw new ArgumentException("AccessToken is required", nameof(AccessToken));
            }

            if (string.IsNullOrWhiteSpace(Owner))
            {
                throw new ArgumentException("Owner is required", nameof(Owner));
            }

            if (string.IsNullOrWhiteSpace(Repo))
            {
                throw new ArgumentException("Repo is required", nameof(Repo));
            }

            if (EnableEncryption && string.IsNullOrWhiteSpace(EncryptionKey))
            {
                throw new ArgumentException("EncryptionKey is required when EnableEncryption is true", nameof(EncryptionKey));
            }
        }
    }
}
