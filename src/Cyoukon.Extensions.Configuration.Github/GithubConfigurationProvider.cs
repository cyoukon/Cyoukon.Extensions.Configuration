using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cyoukon.Extensions.Configuration.Abstractions;
using Cyoukon.Extensions.Configuration.Abstractions.Services;
using Cyoukon.Extensions.Configuration.Github.Models;
using Cyoukon.Extensions.Configuration.Github.Services;

namespace Cyoukon.Extensions.Configuration.Github
{
    public class GithubConfigurationProvider : GitConfigurationProviderBase<GithubConfigurationOptions>
    {
        private readonly IGithubApiClient _githubClient;

        public GithubConfigurationProvider(
            IGithubApiClient githubClient,
            GithubConfigurationOptions options,
            IEncryptionService? encryptionService)
            : base(options, encryptionService)
        {
            _githubClient = githubClient;
        }

        protected override async Task LoadAsync()
        {
            try
            {
                var filePath = Options.GetFilePath();
                var fileContent = await _githubClient.GetFileContentAsync(filePath).ConfigureAwait(false);

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
                SetError($"Failed to load configuration from GitHub: {ex.Message}");
            }
        }

        public override async Task<bool> SaveAsync(Dictionary<string, string?> data, string commitMessage = "Update configuration")
        {
            try
            {
                var filePath = Options.GetFilePath();
                var content = ConfigurationParser.SerializeToJson(data);
                content = ProcessContentBeforeSave(content);

                var response = await _githubClient.CreateOrUpdateFileAsync(
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
