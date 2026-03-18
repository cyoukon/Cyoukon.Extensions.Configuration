using System.Net.Http;
using Cyoukon.Extensions.Configuration.Abstractions.Services;
using Cyoukon.Extensions.Configuration.Github.Services;
using Microsoft.Extensions.Configuration;

namespace Cyoukon.Extensions.Configuration.Github
{
    public class GithubConfigurationSource : IConfigurationSource
    {
        private readonly GithubConfigurationOptions _options;
        private readonly HttpClient? _httpClient;

        public GithubConfigurationSource(GithubConfigurationOptions options, HttpClient? httpClient = null)
        {
            _options = options;
            _httpClient = httpClient;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            var httpClient = _httpClient ?? new HttpClient();
            var githubClient = new GithubApiClient(httpClient, _options);
            IEncryptionService? encryptionService = null;

            if (_options.EnableEncryption && !string.IsNullOrEmpty(_options.EncryptionKey))
            {
                encryptionService = new AesEncryptionService(_options.EncryptionKey);
            }

            return new GithubConfigurationProvider(githubClient, _options, encryptionService);
        }
    }
}
