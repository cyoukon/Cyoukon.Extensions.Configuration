using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Cyoukon.Extensions.Configuration.Abstractions;
using Cyoukon.Extensions.Configuration.Gitee.Models;

namespace Cyoukon.Extensions.Configuration.Gitee
{
    public class GiteeWebhookHandler
    {
        private readonly GiteeConfigurationProvider _provider;
        private readonly GiteeConfigurationOptions _options;
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        public GiteeWebhookHandler(GiteeConfigurationProvider provider, GiteeConfigurationOptions options)
        {
            _provider = provider;
            _options = options;
        }

        public async Task<WebhookResult> HandleWebhookAsync(string payload, string? eventType = null)
        {
            return await HandleWebhookAsync(payload, null, null, eventType).ConfigureAwait(false);
        }

        public async Task<WebhookResult> HandleWebhookAsync(string payload, string? timestamp, string? signature, string? eventType = null)
        {
            if (string.IsNullOrEmpty(payload))
            {
                return new WebhookResult
                {
                    Success = false,
                    Message = "Payload is empty"
                };
            }

            if (!string.IsNullOrEmpty(_options.WebhookSecret))
            {
                if (string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(signature))
                {
                    return new WebhookResult
                    {
                        Success = false,
                        Message = "Missing timestamp or signature for webhook verification"
                    };
                }

                if (!WebhookSignatureVerifier.VerifyHmacSha256Signature(timestamp, signature, _options.WebhookSecret))
                {
                    return new WebhookResult
                    {
                        Success = false,
                        Message = "Webhook signature verification failed"
                    };
                }
            }

            try
            {
                var pushEvent = JsonSerializer.Deserialize<GiteeWebhookPushEvent>(payload, JsonOptions);
                
                if (pushEvent == null)
                {
                    return new WebhookResult
                    {
                        Success = false,
                        Message = "Failed to parse webhook payload"
                    };
                }

                var branch = pushEvent.Ref?.Replace("refs/heads/", "");
                if (!string.Equals(branch, _options.Branch, StringComparison.OrdinalIgnoreCase))
                {
                    return new WebhookResult
                    {
                        Success = true,
                        Message = $"Branch '{branch}' does not match configured branch '{_options.Branch}'",
                        ConfigurationChanged = false
                    };
                }

                var configPathPrefix = _options.ConfigPath.TrimStart('/').TrimEnd('/');
                var changedFiles = GetChangedFiles(pushEvent);
                var configChanged = changedFiles.Any(f => 
                    f.StartsWith(configPathPrefix, StringComparison.OrdinalIgnoreCase) &&
                    f.EndsWith(_options.ConfigFileName, StringComparison.OrdinalIgnoreCase));

                if (configChanged)
                {
                    await _provider.OnWebhookReceivedAsync().ConfigureAwait(false);
                    return new WebhookResult
                    {
                        Success = true,
                        Message = "Configuration reloaded successfully",
                        ConfigurationChanged = true,
                        ChangedFiles = changedFiles
                    };
                }

                return new WebhookResult
                {
                    Success = true,
                    Message = "No configuration files changed",
                    ConfigurationChanged = false,
                    ChangedFiles = changedFiles
                };
            }
            catch (JsonException ex)
            {
                return new WebhookResult
                {
                    Success = false,
                    Message = $"Failed to parse webhook payload: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new WebhookResult
                {
                    Success = false,
                    Message = $"Error processing webhook: {ex.Message}"
                };
            }
        }

        private static string[] GetChangedFiles(GiteeWebhookPushEvent pushEvent)
        {
            var files = new System.Collections.Generic.List<string>();

            if (pushEvent.Commits != null)
            {
                foreach (var commit in pushEvent.Commits)
                {
                    if (commit.Added != null)
                    {
                        files.AddRange(commit.Added);
                    }
                    if (commit.Modified != null)
                    {
                        files.AddRange(commit.Modified);
                    }
                    if (commit.Removed != null)
                    {
                        files.AddRange(commit.Removed);
                    }
                }
            }

            return files.Distinct().ToArray();
        }
    }
}
