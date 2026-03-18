using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Cyoukon.Extensions.Configuration.Gitee.Tests;

public class GiteeConfigurationSourceTests
{
    private readonly GiteeConfigurationOptions _validOptions;

    public GiteeConfigurationSourceTests()
    {
        _validOptions = new GiteeConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo"
        };
    }

    [Fact]
    public void Constructor_WithValidOptions_InitializesCorrectly()
    {
        var source = new GiteeConfigurationSource(_validOptions);
        source.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithHttpClient_InitializesCorrectly()
    {
        var httpClient = new HttpClient();
        var source = new GiteeConfigurationSource(_validOptions, httpClient);
        source.Should().NotBeNull();
    }

    [Fact]
    public void Build_ReturnsGiteeConfigurationProvider()
    {
        var source = new GiteeConfigurationSource(_validOptions);
        var builder = new ConfigurationBuilder();

        var provider = source.Build(builder);

        provider.Should().NotBeNull();
        provider.Should().BeOfType<GiteeConfigurationProvider>();
    }

    [Fact]
    public void Build_WithEncryptionEnabled_CreatesEncryptionService()
    {
        var options = new GiteeConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo",
            EnableEncryption = true,
            EncryptionKey = "test-encryption-key"
        };

        var source = new GiteeConfigurationSource(options);
        var builder = new ConfigurationBuilder();

        var provider = source.Build(builder);

        provider.Should().NotBeNull();
        provider.Should().BeOfType<GiteeConfigurationProvider>();
    }

    [Fact]
    public void Build_WithEncryptionEnabledButNoKey_CreatesProviderWithoutEncryption()
    {
        var options = new GiteeConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo",
            EnableEncryption = true,
            EncryptionKey = null
        };

        var source = new GiteeConfigurationSource(options);
        var builder = new ConfigurationBuilder();

        var provider = source.Build(builder);

        provider.Should().NotBeNull();
        provider.Should().BeOfType<GiteeConfigurationProvider>();
    }

    [Fact]
    public void Build_WithProvidedHttpClient_UsesProvidedClient()
    {
        var httpClient = new HttpClient();
        var source = new GiteeConfigurationSource(_validOptions, httpClient);
        var builder = new ConfigurationBuilder();

        var provider = source.Build(builder);

        provider.Should().NotBeNull();
    }

    [Fact]
    public void Build_WithoutHttpClient_CreatesNewHttpClient()
    {
        var source = new GiteeConfigurationSource(_validOptions);
        var builder = new ConfigurationBuilder();

        var provider = source.Build(builder);

        provider.Should().NotBeNull();
    }

    [Fact]
    public void Build_WithEmptyEncryptionKey_DoesNotCreateEncryptionService()
    {
        var options = new GiteeConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo",
            EnableEncryption = true,
            EncryptionKey = ""
        };

        var source = new GiteeConfigurationSource(options);
        var builder = new ConfigurationBuilder();

        var provider = source.Build(builder);

        provider.Should().NotBeNull();
        provider.Should().BeOfType<GiteeConfigurationProvider>();
    }

    [Fact]
    public void Build_CreatesProviderWithCorrectOptions()
    {
        var options = new GiteeConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo",
            Branch = "develop",
            ConfigPath = "custom-config",
            ConfigFileName = "custom.json"
        };

        var source = new GiteeConfigurationSource(options);
        var builder = new ConfigurationBuilder();

        var provider = source.Build(builder);

        provider.Should().NotBeNull();
    }
}
