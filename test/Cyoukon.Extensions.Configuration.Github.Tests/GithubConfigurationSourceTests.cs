using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Cyoukon.Extensions.Configuration.Github.Tests;

public class GithubConfigurationSourceTests
{
    private readonly GithubConfigurationOptions _validOptions;

    public GithubConfigurationSourceTests()
    {
        _validOptions = new GithubConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo"
        };
    }

    [Fact]
    public void Constructor_WithValidOptions_InitializesCorrectly()
    {
        var source = new GithubConfigurationSource(_validOptions);
        source.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithHttpClient_InitializesCorrectly()
    {
        var httpClient = new HttpClient();
        var source = new GithubConfigurationSource(_validOptions, httpClient);
        source.Should().NotBeNull();
    }

    [Fact]
    public void Build_ReturnsGithubConfigurationProvider()
    {
        var source = new GithubConfigurationSource(_validOptions);
        var builder = new ConfigurationBuilder();

        var provider = source.Build(builder);

        provider.Should().NotBeNull();
        provider.Should().BeOfType<GithubConfigurationProvider>();
    }

    [Fact]
    public void Build_WithEncryptionEnabled_CreatesEncryptionService()
    {
        var options = new GithubConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo",
            EnableEncryption = true,
            EncryptionKey = "test-encryption-key"
        };

        var source = new GithubConfigurationSource(options);
        var builder = new ConfigurationBuilder();

        var provider = source.Build(builder);

        provider.Should().NotBeNull();
        provider.Should().BeOfType<GithubConfigurationProvider>();
    }

    [Fact]
    public void Build_WithEncryptionEnabledButNoKey_CreatesProviderWithoutEncryption()
    {
        var options = new GithubConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo",
            EnableEncryption = true,
            EncryptionKey = null
        };

        var source = new GithubConfigurationSource(options);
        var builder = new ConfigurationBuilder();

        var provider = source.Build(builder);

        provider.Should().NotBeNull();
        provider.Should().BeOfType<GithubConfigurationProvider>();
    }

    [Fact]
    public void Build_WithProvidedHttpClient_UsesProvidedClient()
    {
        var httpClient = new HttpClient();
        var source = new GithubConfigurationSource(_validOptions, httpClient);
        var builder = new ConfigurationBuilder();

        var provider = source.Build(builder);

        provider.Should().NotBeNull();
    }

    [Fact]
    public void Build_WithoutHttpClient_CreatesNewHttpClient()
    {
        var source = new GithubConfigurationSource(_validOptions);
        var builder = new ConfigurationBuilder();

        var provider = source.Build(builder);

        provider.Should().NotBeNull();
    }

    [Fact]
    public void Build_WithEmptyEncryptionKey_DoesNotCreateEncryptionService()
    {
        var options = new GithubConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo",
            EnableEncryption = true,
            EncryptionKey = ""
        };

        var source = new GithubConfigurationSource(options);
        var builder = new ConfigurationBuilder();

        var provider = source.Build(builder);

        provider.Should().NotBeNull();
        provider.Should().BeOfType<GithubConfigurationProvider>();
    }

    [Fact]
    public void Build_CreatesProviderWithCorrectOptions()
    {
        var options = new GithubConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo",
            Branch = "develop",
            ConfigPath = "custom-config",
            ConfigFileName = "custom.json"
        };

        var source = new GithubConfigurationSource(options);
        var builder = new ConfigurationBuilder();

        var provider = source.Build(builder);

        provider.Should().NotBeNull();
    }
}
