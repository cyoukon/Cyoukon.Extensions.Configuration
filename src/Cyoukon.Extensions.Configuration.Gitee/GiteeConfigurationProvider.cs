using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Cyoukon.Extensions.Configuration.Abstractions;
using Cyoukon.Extensions.Configuration.Abstractions.Services;
using Cyoukon.Extensions.Configuration.Gitee.Models;
using Cyoukon.Extensions.Configuration.Gitee.Services;

namespace Cyoukon.Extensions.Configuration.Gitee
{
    public class GiteeConfigurationProvider : GitConfigurationProviderBase<GiteeConfigurationOptions>
    {
        private readonly IGiteeApiClient _giteeClient;
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        public GiteeConfigurationProvider(
            IGiteeApiClient giteeClient,
            GiteeConfigurationOptions options,
            IEncryptionService? encryptionService)
            : base(options, encryptionService)
        {
            _giteeClient = giteeClient;
        }

        protected override async Task LoadAsync()
        {
            try
            {
                var filePath = Options.GetFilePath();
                var fileContent = await _giteeClient.GetFileContentAsync(filePath).ConfigureAwait(false);

                if (fileContent?.Content != null)
                {
                    CurrentSha = fileContent.Sha;
                    var content = ConfigurationParser.DecodeBase64Content(fileContent.Content);
                    content = ProcessContentBeforeLoad(content);
                    var data = ConfigurationParser.ParseJsonContent(content);
                    SetData(data, fireReload: true);
                }
                else
                {
                    Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
                }
            }
            catch (Exception ex)
            {
                SetError($"Failed to load configuration from Gitee: {ex.Message}");
            }
        }

        public override async Task<bool> SaveAsync(Dictionary<string, string?> data, string commitMessage = "Update configuration")
        {
            try
            {
                var filePath = Options.GetFilePath();
                var content = ConfigurationParser.SerializeToJson(data);
                content = ProcessContentBeforeSave(content);

                var response = await _giteeClient.CreateOrUpdateFileAsync(
                    filePath,
                    content,
                    commitMessage,
                    CurrentSha).ConfigureAwait(false);

                if (response?.Content != null)
                {
                    CurrentSha = response.Content.Sha;
                    SetData(data, fireReload: true);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
