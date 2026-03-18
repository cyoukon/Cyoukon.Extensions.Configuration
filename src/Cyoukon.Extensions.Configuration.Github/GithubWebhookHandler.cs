using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Cyoukon.Extensions.Configuration.Abstractions;
using Cyoukon.Extensions.Configuration.Github.Models;

namespace Cyoukon.Extensions.Configuration.Github
{
    public class GithubWebhookHandler
    {
        private readonly GithubConfigurationProvider _provider;
        private readonly GithubConfigurationOptions _options;
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        public GithubWebhookHandler(GithubConfigurationProvider provider, GithubConfigurationOptions options)
        {
            _provider = provider;
            _options = options;
        }

        public async Task<WebhookResult> HandleWebhookAsync(string payload, string? signature = null)
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
                if (string.IsNullOrEmpty(signature))
                {
                    return new WebhookResult
                    {
                        Success = false,
                        Message = "Missing signature for webhook verification"
                    };
                }

                if (!WebhookSignatureVerifier.VerifySha256Signature(payload, signature, _options.WebhookSecret))
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
                var pushEvent = JsonSerializer.Deserialize<GithubWebhookPushEvent>(payload, JsonOptions);
                
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

        private static string[] GetChangedFiles(GithubWebhookPushEvent pushEvent)
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
