using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cyoukon.Extensions.Configuration.Abstractions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Cyoukon.Extensions.Configuration.Abstractions
{
    public abstract class GitConfigurationProviderBase<TOptions> : ConfigurationProvider, IDisposable
        where TOptions : GitRepositoryOptions
    {
        protected readonly TOptions Options;
        protected readonly IEncryptionService? EncryptionService;
        private readonly CancellationTokenSource _reloadCancellationTokenSource;
        private readonly TimeSpan _reloadInterval;
        private Task? _reloadTask;
        protected string? CurrentSha;
        private bool _disposed;

        protected GitConfigurationProviderBase(TOptions options, IEncryptionService? encryptionService)
        {
            Options = options;
            EncryptionService = encryptionService;
            _reloadInterval = options.ReloadInterval;
            _reloadCancellationTokenSource = new CancellationTokenSource();

            if (options.EnableReload)
            {
                _reloadTask = ReloadLoop(_reloadCancellationTokenSource.Token);
            }
        }

        public override void Load()
        {
            LoadAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        protected abstract Task LoadAsync();

        public abstract Task<bool> SaveAsync(Dictionary<string, string?> data, string commitMessage = "Update configuration");

        public async Task<bool> SetAsync(string key, string? value, string commitMessage = "Update configuration key")
        {
            var data = new Dictionary<string, string?>(Data, StringComparer.OrdinalIgnoreCase)
            {
                [key] = value
            };
            return await SaveAsync(data, commitMessage).ConfigureAwait(false);
        }

        public async Task<bool> RemoveAsync(string key, string commitMessage = "Remove configuration key")
        {
            var data = new Dictionary<string, string?>(Data, StringComparer.OrdinalIgnoreCase);
            data.Remove(key);
            return await SaveAsync(data, commitMessage).ConfigureAwait(false);
        }

        public async Task ReloadAsync()
        {
            await LoadAsync().ConfigureAwait(false);
            OnReload();
        }

        public async Task OnWebhookReceivedAsync()
        {
            await LoadAsync().ConfigureAwait(false);
            OnReload();
        }

        private async Task ReloadLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_reloadInterval, cancellationToken).ConfigureAwait(false);
                    await LoadAsync().ConfigureAwait(false);
                    OnReload();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                }
            }
        }

        protected void SetData(Dictionary<string, string?> data, bool fireReload)
        {
            Data = new Dictionary<string, string?>(data, StringComparer.OrdinalIgnoreCase);
            if (fireReload)
            {
                OnReload();
            }
        }

        protected string ProcessContentBeforeLoad(string content)
        {
            if (Options.EnableEncryption && EncryptionService != null)
            {
                return EncryptionService.Decrypt(content);
            }
            return content;
        }

        protected string ProcessContentBeforeSave(string content)
        {
            if (Options.EnableEncryption && EncryptionService != null)
            {
                return EncryptionService.Encrypt(content);
            }
            return content;
        }

        protected void SetError(string message)
        {
            Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["GitConfiguration:Error"] = message
            };
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _reloadCancellationTokenSource.Cancel();
                _reloadTask?.Wait(TimeSpan.FromSeconds(5));
                _reloadCancellationTokenSource.Dispose();
                _disposed = true;
            }
        }
    }
}
