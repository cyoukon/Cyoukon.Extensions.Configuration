using System.Threading;
using System.Threading.Tasks;
using Cyoukon.Extensions.Configuration.Github.Models;

namespace Cyoukon.Extensions.Configuration.Github.Services
{
    public interface IGithubApiClient
    {
        Task<GithubFileContent?> GetFileContentAsync(string path, CancellationToken cancellationToken = default);
        Task<GithubCommitResponse?> CreateOrUpdateFileAsync(string path, string content, string message, string? sha = null, CancellationToken cancellationToken = default);
        Task<bool> DeleteFileAsync(string path, string message, string sha, CancellationToken cancellationToken = default);
    }
}
