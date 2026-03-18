using Cyoukon.Extensions.Configuration.Abstractions;
using Cyoukon.Extensions.Configuration.Github.Models;
using Cyoukon.Extensions.Configuration.Github.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace Cyoukon.Extensions.Configuration.Github.Tests;

public class GithubWebhookHandlerTests
{
    private readonly Mock<IGithubApiClient> _mockApiClient;
    private readonly GithubConfigurationOptions _options;
    private readonly GithubConfigurationProvider _provider;

    public GithubWebhookHandlerTests()
    {
        _mockApiClient = new Mock<IGithubApiClient>();
        _options = new GithubConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo",
            Branch = "main"
        };
        _provider = new GithubConfigurationProvider(_mockApiClient.Object, _options, null);
    }

    [Fact]
    public void Constructor_WithValidParameters_InitializesCorrectly()
    {
        var handler = new GithubWebhookHandler(_provider, _options);
        handler.Should().NotBeNull();
    }

    [Fact]
    public async Task HandleWebhookAsync_WithEmptyPayload_ReturnsFailure()
    {
        var handler = new GithubWebhookHandler(_provider, _options);
        var result = await handler.HandleWebhookAsync("").ConfigureAwait(false);
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Payload is empty");
    }

    [Fact]
    public async Task HandleWebhookAsync_WithNullPayload_ReturnsFailure()
    {
        var handler = new GithubWebhookHandler(_provider, _options);
        var result = await handler.HandleWebhookAsync(null!).ConfigureAwait(false);
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Payload is empty");
    }

    [Fact]
    public async Task HandleWebhookAsync_WithValidPayload_ReturnsSuccess()
    {
        var payload = "{\"ref\":\"refs/heads/main\",\"commits\":[]}";
        var handler = new GithubWebhookHandler(_provider, _options);
        var result = await handler.HandleWebhookAsync(payload).ConfigureAwait(false);
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task HandleWebhookAsync_WithDifferentBranch_ReturnsSuccessWithNoChange()
    {
        var payload = "{\"ref\":\"refs/heads/develop\",\"commits\":[]}";
        var handler = new GithubWebhookHandler(_provider, _options);
        var result = await handler.HandleWebhookAsync(payload).ConfigureAwait(false);
        result.Success.Should().BeTrue();
        result.ConfigurationChanged.Should().BeFalse();
        result.Message.Should().Contain("does not match configured branch");
    }

    [Fact]
    public async Task HandleWebhookAsync_WithConfigFileChange_ReturnsConfigurationChanged()
    {
        var payload = "{\"ref\":\"refs/heads/main\",\"commits\":[{\"added\":[\"config/appsettings.json\"],\"modified\":[],\"removed\":[]}]}";
        var handler = new GithubWebhookHandler(_provider, _options);
        var result = await handler.HandleWebhookAsync(payload).ConfigureAwait(false);
        result.Success.Should().BeTrue();
        result.ConfigurationChanged.Should().BeTrue();
        result.Message.Should().Be("Configuration reloaded successfully");
        result.ChangedFiles.Should().Contain("config/appsettings.json");
    }

    [Fact]
    public async Task HandleWebhookAsync_WithModifiedConfigFile_ReturnsConfigurationChanged()
    {
        var payload = "{\"ref\":\"refs/heads/main\",\"commits\":[{\"added\":[],\"modified\":[\"config/appsettings.json\"],\"removed\":[]}]}";
        var handler = new GithubWebhookHandler(_provider, _options);
        var result = await handler.HandleWebhookAsync(payload).ConfigureAwait(false);
        result.Success.Should().BeTrue();
        result.ConfigurationChanged.Should().BeTrue();
    }

    [Fact]
    public async Task HandleWebhookAsync_WithRemovedConfigFile_ReturnsConfigurationChanged()
    {
        var payload = "{\"ref\":\"refs/heads/main\",\"commits\":[{\"added\":[],\"modified\":[],\"removed\":[\"config/appsettings.json\"]}]}";
        var handler = new GithubWebhookHandler(_provider, _options);
        var result = await handler.HandleWebhookAsync(payload).ConfigureAwait(false);
        result.Success.Should().BeTrue();
        result.ConfigurationChanged.Should().BeTrue();
    }

    [Fact]
    public async Task HandleWebhookAsync_WithNonConfigFileChange_ReturnsNoConfigurationChanged()
    {
        var payload = "{\"ref\":\"refs/heads/main\",\"commits\":[{\"added\":[\"other/file.txt\"],\"modified\":[],\"removed\":[]}]}";
        var handler = new GithubWebhookHandler(_provider, _options);
        var result = await handler.HandleWebhookAsync(payload).ConfigureAwait(false);
        result.Success.Should().BeTrue();
        result.ConfigurationChanged.Should().BeFalse();
        result.Message.Should().Be("No configuration files changed");
    }

    [Fact]
    public async Task HandleWebhookAsync_WithInvalidJson_ReturnsFailure()
    {
        var payload = "not valid json";
        var handler = new GithubWebhookHandler(_provider, _options);
        var result = await handler.HandleWebhookAsync(payload).ConfigureAwait(false);
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Failed to parse webhook payload");
    }

    [Fact]
    public async Task HandleWebhookAsync_WithWebhookSecret_MissingSignature_ReturnsFailure()
    {
        var options = new GithubConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo",
            Branch = "main",
            WebhookSecret = "my-secret"
        };
        var provider = new GithubConfigurationProvider(_mockApiClient.Object, options, null);
        var handler = new GithubWebhookHandler(provider, options);

        var payload = "{\"ref\":\"refs/heads/main\",\"commits\":[]}";
        var result = await handler.HandleWebhookAsync(payload, null).ConfigureAwait(false);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Missing signature for webhook verification");
    }

    [Fact]
    public async Task HandleWebhookAsync_WithWebhookSecret_InvalidSignature_ReturnsFailure()
    {
        var options = new GithubConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo",
            Branch = "main",
            WebhookSecret = "my-secret"
        };
        var provider = new GithubConfigurationProvider(_mockApiClient.Object, options, null);
        var handler = new GithubWebhookHandler(provider, options);

        var payload = "{\"ref\":\"refs/heads/main\",\"commits\":[]}";
        var result = await handler.HandleWebhookAsync(payload, "sha256=invalid-signature").ConfigureAwait(false);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Webhook signature verification failed");
    }

    [Fact]
    public async Task HandleWebhookAsync_WithWebhookSecret_ValidSignature_ReturnsSuccess()
    {
        var options = new GithubConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo",
            Branch = "main",
            WebhookSecret = "my-secret"
        };
        var provider = new GithubConfigurationProvider(_mockApiClient.Object, options, null);
        var handler = new GithubWebhookHandler(provider, options);

        var payload = "{\"ref\":\"refs/heads/main\",\"commits\":[]}";
        var signature = WebhookSignatureVerifier.ComputeSha256Signature(payload, options.WebhookSecret);
        var result = await handler.HandleWebhookAsync(payload, signature).ConfigureAwait(false);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task HandleWebhookAsync_WithMultipleCommits_AggregatesChangedFiles()
    {
        var payload = "{\"ref\":\"refs/heads/main\",\"commits\":[{\"added\":[\"file1.txt\"],\"modified\":[],\"removed\":[]},{\"added\":[],\"modified\":[\"file2.txt\"],\"removed\":[]},{\"added\":[],\"modified\":[],\"removed\":[\"file3.txt\"]}]}";
        var handler = new GithubWebhookHandler(_provider, _options);
        var result = await handler.HandleWebhookAsync(payload).ConfigureAwait(false);

        result.Success.Should().BeTrue();
        result.ChangedFiles.Should().Contain("file1.txt");
        result.ChangedFiles.Should().Contain("file2.txt");
        result.ChangedFiles.Should().Contain("file3.txt");
    }

    [Fact]
    public async Task HandleWebhookAsync_WithDuplicateFiles_RemovesDuplicates()
    {
        var payload = "{\"ref\":\"refs/heads/main\",\"commits\":[{\"added\":[\"file.txt\"],\"modified\":[\"file.txt\"],\"removed\":[]}]}";
        var handler = new GithubWebhookHandler(_provider, _options);
        var result = await handler.HandleWebhookAsync(payload).ConfigureAwait(false);

        result.Success.Should().BeTrue();
        result.ChangedFiles.Should().HaveCount(1);
    }

    [Fact]
    public async Task HandleWebhookAsync_WithNullCommits_HandlesGracefully()
    {
        var payload = "{\"ref\":\"refs/heads/main\",\"commits\":null}";
        var handler = new GithubWebhookHandler(_provider, _options);
        var result = await handler.HandleWebhookAsync(payload).ConfigureAwait(false);

        result.Success.Should().BeTrue();
        result.ConfigurationChanged.Should().BeFalse();
    }

    [Fact]
    public async Task HandleWebhookAsync_WithEmptyCommits_HandlesGracefully()
    {
        var payload = "{\"ref\":\"refs/heads/main\",\"commits\":[]}";
        var handler = new GithubWebhookHandler(_provider, _options);
        var result = await handler.HandleWebhookAsync(payload).ConfigureAwait(false);

        result.Success.Should().BeTrue();
        result.ConfigurationChanged.Should().BeFalse();
    }

    [Fact]
    public async Task HandleWebhookAsync_WithCustomConfigPath_DetectsChanges()
    {
        var options = new GithubConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo",
            Branch = "main",
            ConfigPath = "myconfig",
            ConfigFileName = "settings.json"
        };
        var provider = new GithubConfigurationProvider(_mockApiClient.Object, options, null);
        var handler = new GithubWebhookHandler(provider, options);

        var payload = "{\"ref\":\"refs/heads/main\",\"commits\":[{\"added\":[\"myconfig/settings.json\"],\"modified\":[],\"removed\":[]}]}";
        var result = await handler.HandleWebhookAsync(payload).ConfigureAwait(false);

        result.Success.Should().BeTrue();
        result.ConfigurationChanged.Should().BeTrue();
    }

    [Fact]
    public async Task HandleWebhookAsync_WithLeadingSlashConfigPath_DetectsChanges()
    {
        var options = new GithubConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo",
            Branch = "main",
            ConfigPath = "/config"
        };
        var provider = new GithubConfigurationProvider(_mockApiClient.Object, options, null);
        var handler = new GithubWebhookHandler(provider, options);

        var payload = "{\"ref\":\"refs/heads/main\",\"commits\":[{\"added\":[\"config/appsettings.json\"],\"modified\":[],\"removed\":[]}]}";
        var result = await handler.HandleWebhookAsync(payload).ConfigureAwait(false);

        result.Success.Should().BeTrue();
        result.ConfigurationChanged.Should().BeTrue();
    }

    [Fact]
    public async Task HandleWebhookAsync_WithNullRef_HandlesGracefully()
    {
        var payload = "{\"ref\":null,\"commits\":[]}";
        var handler = new GithubWebhookHandler(_provider, _options);
        var result = await handler.HandleWebhookAsync(payload).ConfigureAwait(false);

        result.Success.Should().BeTrue();
        result.ConfigurationChanged.Should().BeFalse();
    }

    [Fact]
    public async Task HandleWebhookAsync_WithCaseInsensitiveBranch_Matches()
    {
        var payload = "{\"ref\":\"refs/heads/MAIN\",\"commits\":[]}";
        var handler = new GithubWebhookHandler(_provider, _options);
        var result = await handler.HandleWebhookAsync(payload).ConfigureAwait(false);

        result.Success.Should().BeTrue();
        result.ConfigurationChanged.Should().BeFalse();
    }

    [Fact]
    public async Task HandleWebhookAsync_WithNullAdded_HandlesGracefully()
    {
        var payload = "{\"ref\":\"refs/heads/main\",\"commits\":[{\"added\":null,\"modified\":[],\"removed\":[]}]}";
        var handler = new GithubWebhookHandler(_provider, _options);
        var result = await handler.HandleWebhookAsync(payload).ConfigureAwait(false);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task HandleWebhookAsync_WithNullModified_HandlesGracefully()
    {
        var payload = "{\"ref\":\"refs/heads/main\",\"commits\":[{\"added\":[],\"modified\":null,\"removed\":[]}]}";
        var handler = new GithubWebhookHandler(_provider, _options);
        var result = await handler.HandleWebhookAsync(payload).ConfigureAwait(false);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task HandleWebhookAsync_WithNullRemoved_HandlesGracefully()
    {
        var payload = "{\"ref\":\"refs/heads/main\",\"commits\":[{\"added\":[],\"modified\":[],\"removed\":null}]}";
        var handler = new GithubWebhookHandler(_provider, _options);
        var result = await handler.HandleWebhookAsync(payload).ConfigureAwait(false);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task HandleWebhookAsync_WithEmptySignature_WhenSecretRequired_ReturnsFailure()
    {
        var options = new GithubConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo",
            Branch = "main",
            WebhookSecret = "my-secret"
        };
        var provider = new GithubConfigurationProvider(_mockApiClient.Object, options, null);
        var handler = new GithubWebhookHandler(provider, options);

        var payload = "{\"ref\":\"refs/heads/main\",\"commits\":[]}";
        var result = await handler.HandleWebhookAsync(payload, "").ConfigureAwait(false);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Missing signature for webhook verification");
    }
}
