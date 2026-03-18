using System.Text.Json.Serialization;

namespace Cyoukon.Extensions.Configuration.Github.Models
{
    public class GithubFileContent
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("sha")]
        public string? Sha { get; set; }

        [JsonPropertyName("path")]
        public string? Path { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("encoding")]
        public string? Encoding { get; set; }

        [JsonPropertyName("download_url")]
        public string? DownloadUrl { get; set; }
    }

    internal class GithubCommitRequest
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("sha")]
        public string? Sha { get; set; }

        [JsonPropertyName("branch")]
        public string Branch { get; set; } = "main";
    }

    public class GithubCommitResponse
    {
        [JsonPropertyName("content")]
        public GithubFileContent? Content { get; set; }

        [JsonPropertyName("commit")]
        public GithubCommitInfo? Commit { get; set; }
    }

    public class GithubCommitInfo
    {
        [JsonPropertyName("sha")]
        public string? Sha { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    internal class GithubWebhookPushEvent
    {
        [JsonPropertyName("ref")]
        public string? Ref { get; set; }

        [JsonPropertyName("before")]
        public string? Before { get; set; }

        [JsonPropertyName("after")]
        public string? After { get; set; }

        [JsonPropertyName("commits")]
        public GithubCommitDetail[]? Commits { get; set; }

        [JsonPropertyName("repository")]
        public GithubRepository? Repository { get; set; }
    }

    internal class GithubCommitDetail
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("added")]
        public string[]? Added { get; set; }

        [JsonPropertyName("modified")]
        public string[]? Modified { get; set; }

        [JsonPropertyName("removed")]
        public string[]? Removed { get; set; }
    }

    internal class GithubRepository
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("full_name")]
        public string? FullName { get; set; }
    }
}
