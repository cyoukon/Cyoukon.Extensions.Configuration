using System.Threading;
using System.Threading.Tasks;
using Cyoukon.Extensions.Configuration.Gitee.Models;

namespace Cyoukon.Extensions.Configuration.Gitee.Services
{
    public interface IGiteeApiClient
    {
        Task<GiteeFileContent?> GetFileContentAsync(string path, CancellationToken cancellationToken = default);
        Task<GiteeCommitResponse?> CreateOrUpdateFileAsync(string path, string content, string message, string? sha = null, CancellationToken cancellationToken = default);
        Task<bool> DeleteFileAsync(string path, string message, string sha, CancellationToken cancellationToken = default);
    }
}
