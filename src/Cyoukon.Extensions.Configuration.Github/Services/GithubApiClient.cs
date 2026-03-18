using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Cyoukon.Extensions.Configuration.Github.Models;

namespace Cyoukon.Extensions.Configuration.Github.Services
{
    public class GithubApiClient : IGithubApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly GithubConfigurationOptions _options;
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        public GithubApiClient(HttpClient httpClient, GithubConfigurationOptions options)
        {
            _httpClient = httpClient;
            _options = options;
            ConfigureHttpClient();
        }

        private void ConfigureHttpClient()
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);
            _httpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", _options.ApiVersion);
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Cyoukon.Extensions.Configuration.Github", "1.0.0"));
        }

        public async Task<GithubFileContent?> GetFileContentAsync(string path, CancellationToken cancellationToken = default)
        {
            var url = $"{_options.ApiBaseUrl}/repos/{_options.Owner}/{_options.Repo}/contents/{path}?ref={_options.Branch}";

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
            return JsonSerializer.Deserialize<GithubFileContent>(content, JsonOptions);
        }

        public async Task<GithubCommitResponse?> CreateOrUpdateFileAsync(string path, string content, string message, string? sha = null, CancellationToken cancellationToken = default)
        {
            var url = $"{_options.ApiBaseUrl}/repos/{_options.Owner}/{_options.Repo}/contents/{path}";

            var requestBody = new GithubCommitRequest
            {
                Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(content)),
                Message = message,
                Sha = sha,
                Branch = _options.Branch
            };

            var jsonContent = JsonSerializer.Serialize(requestBody, JsonOptions);
            using var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Put, url)
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
            return JsonSerializer.Deserialize<GithubCommitResponse>(responseContent, JsonOptions);
        }

        public async Task<bool> DeleteFileAsync(string path, string message, string sha, CancellationToken cancellationToken = default)
        {
            var url = $"{_options.ApiBaseUrl}/repos/{_options.Owner}/{_options.Repo}/contents/{path}";

            var requestBody = new GithubCommitRequest
            {
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
