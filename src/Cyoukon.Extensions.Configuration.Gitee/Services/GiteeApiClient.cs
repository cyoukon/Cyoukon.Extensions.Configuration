using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Cyoukon.Extensions.Configuration.Gitee.Models;

namespace Cyoukon.Extensions.Configuration.Gitee.Services
{
    public class GiteeApiClient : IGiteeApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly GiteeConfigurationOptions _options;
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        public GiteeApiClient(HttpClient httpClient, GiteeConfigurationOptions options)
        {
            _httpClient = httpClient;
            _options = options;
        }

        public async Task<GiteeFileContent?> GetFileContentAsync(string path, CancellationToken cancellationToken = default)
        {
            var url = $"{_options.ApiBaseUrl}/repos/{_options.Owner}/{_options.Repo}/contents/{path}?access_token={_options.AccessToken}&ref={_options.Branch}";

            var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                var errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new HttpRequestException($"Failed to get file content. Status: {response.StatusCode}, Error: {errorContent}");
            }

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<GiteeFileContent>(content, JsonOptions);
        }

        public async Task<GiteeCommitResponse?> CreateOrUpdateFileAsync(string path, string content, string message, string? sha = null, CancellationToken cancellationToken = default)
        {
            var url = $"{_options.ApiBaseUrl}/repos/{_options.Owner}/{_options.Repo}/contents/{path}";

            var requestBody = new GiteeCommitRequest
            {
                AccessToken = _options.AccessToken,
                Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(content)),
                Message = message,
                Sha = sha,
                Branch = _options.Branch
            };

            var jsonContent = JsonSerializer.Serialize(requestBody, JsonOptions);
            using var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var method = sha != null ? HttpMethod.Put : HttpMethod.Post;
            var request = new HttpRequestMessage(method, url)
            {
                Content = httpContent
            };

            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new HttpRequestException($"Failed to create/update file. Status: {response.StatusCode}, Error: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<GiteeCommitResponse>(responseContent, JsonOptions);
        }

        public async Task<bool> DeleteFileAsync(string path, string message, string sha, CancellationToken cancellationToken = default)
        {
            var url = $"{_options.ApiBaseUrl}/repos/{_options.Owner}/{_options.Repo}/contents/{path}";

            var requestBody = new GiteeCommitRequest
            {
                AccessToken = _options.AccessToken,
                Message = message,
                Sha = sha,
                Branch = _options.Branch
            };

            var jsonContent = JsonSerializer.Serialize(requestBody, JsonOptions);
            using var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Delete, url)
            {
                Content = httpContent
            };

            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }
    }
}
