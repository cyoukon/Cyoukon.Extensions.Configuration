using System.Net.Http;
using Cyoukon.Extensions.Configuration.Abstractions.Services;
using Cyoukon.Extensions.Configuration.Gitee.Services;
using Microsoft.Extensions.Configuration;

namespace Cyoukon.Extensions.Configuration.Gitee
{
    public class GiteeConfigurationSource : IConfigurationSource
    {
        private readonly GiteeConfigurationOptions _options;
        private readonly HttpClient? _httpClient;

        public GiteeConfigurationSource(GiteeConfigurationOptions options, HttpClient? httpClient = null)
        {
            _options = options;
            _httpClient = httpClient;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            var httpClient = _httpClient ?? new HttpClient();
            var giteeClient = new GiteeApiClient(httpClient, _options);
            IEncryptionService? encryptionService = null;

            if (_options.EnableEncryption && !string.IsNullOrEmpty(_options.EncryptionKey))
            {
                encryptionService = new AesEncryptionService(_options.EncryptionKey);
            }

            return new GiteeConfigurationProvider(giteeClient, _options, encryptionService);
        }
    }
}
