using Cyoukon.Extensions.Configuration.Abstractions;

namespace Cyoukon.Extensions.Configuration.Github
{
    public class GithubConfigurationOptions : GitRepositoryOptions
    {
        public string ApiBaseUrl { get; set; } = "https://api.github.com";
        public string ApiVersion { get; set; } = "2022-11-28";
    }
}
