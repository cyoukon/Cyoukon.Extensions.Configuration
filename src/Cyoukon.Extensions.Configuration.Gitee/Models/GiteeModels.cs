using System.Text.Json.Serialization;

namespace Cyoukon.Extensions.Configuration.Gitee.Models
{
    public class GiteeFileContent
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
    }

    internal class GiteeCommitRequest
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("sha")]
        public string? Sha { get; set; }

        [JsonPropertyName("branch")]
        public string Branch { get; set; } = "master";
    }

    public class GiteeCommitResponse
    {
        [JsonPropertyName("content")]
        public GiteeFileContent? Content { get; set; }

        [JsonPropertyName("commit")]
        public GiteeCommitInfo? Commit { get; set; }
    }

    public class GiteeCommitInfo
    {
        [JsonPropertyName("sha")]
        public string? Sha { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    internal class GiteeWebhookPushEvent
    {
        [JsonPropertyName("ref")]
        public string? Ref { get; set; }

        [JsonPropertyName("before")]
        public string? Before { get; set; }

        [JsonPropertyName("after")]
        public string? After { get; set; }

        [JsonPropertyName("commits")]
        public GiteeCommitDetail[]? Commits { get; set; }

        [JsonPropertyName("repository")]
        public GiteeRepository? Repository { get; set; }
    }

    internal class GiteeCommitDetail
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

    internal class GiteeRepository
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("full_name")]
        public string? FullName { get; set; }

        [JsonPropertyName("path_with_namespace")]
        public string? PathWithNamespace { get; set; }
    }
}
