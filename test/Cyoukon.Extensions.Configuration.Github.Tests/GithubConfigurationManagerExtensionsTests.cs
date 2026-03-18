using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Cyoukon.Extensions.Configuration.Github.Tests;

public class GithubConfigurationManagerExtensionsTests
{
    private readonly GithubConfigurationOptions _validOptions;

    public GithubConfigurationManagerExtensionsTests()
    {
        _validOptions = new GithubConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo"
        };
    }

    [Fact]
    public void AddGithubConfiguration_WithAction_ReturnsConfigurationManager()
    {
        var configurationManager = new ConfigurationManager();

        var result = configurationManager.AddGithubConfiguration(options =>
        {
            options.AccessToken = "test-token";
            options.Owner = "test-owner";
            options.Repo = "test-repo";
        });

        result.Should().NotBeNull();
        result.Should().Be(configurationManager);
    }

    [Fact]
    public void AddGithubConfiguration_WithActionAndHttpClient_ReturnsConfigurationManager()
    {
        var configurationManager = new ConfigurationManager();
        var httpClient = new HttpClient();

        var result = configurationManager.AddGithubConfiguration(options =>
        {
            options.AccessToken = "test-token";
            options.Owner = "test-owner";
            options.Repo = "test-repo";
        }, httpClient);

        result.Should().NotBeNull();
        result.Should().Be(configurationManager);
    }

    [Fact]
    public void AddGithubConfiguration_WithOptions_ReturnsConfigurationManager()
    {
        var configurationManager = new ConfigurationManager();

        var result = configurationManager.AddGithubConfiguration(_validOptions);

        result.Should().NotBeNull();
        result.Should().Be(configurationManager);
    }

    [Fact]
    public void AddGithubConfiguration_WithOptionsAndHttpClient_ReturnsConfigurationManager()
    {
        var configurationManager = new ConfigurationManager();
        var httpClient = new HttpClient();

        var result = configurationManager.AddGithubConfiguration(_validOptions, httpClient);

        result.Should().NotBeNull();
        result.Should().Be(configurationManager);
    }

    [Fact]
    public void AddGithubConfiguration_WithInvalidOptions_ThrowsArgumentException()
    {
        var configurationManager = new ConfigurationManager();
        var invalidOptions = new GithubConfigurationOptions
        {
            AccessToken = "",
            Owner = "test-owner",
            Repo = "test-repo"
        };

        var act = () => configurationManager.AddGithubConfiguration(invalidOptions);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddGithubConfiguration_WithInvalidActionOptions_ThrowsArgumentException()
    {
        var configurationManager = new ConfigurationManager();

        var act = () => configurationManager.AddGithubConfiguration(options =>
        {
            options.AccessToken = "";
            options.Owner = "test-owner";
            options.Repo = "test-repo";
        });
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddGithubConfiguration_WithSectionName_ThrowsWhenSectionNotFound()
    {
        var configurationManager = new ConfigurationManager();

        var act = () => configurationManager.AddGithubConfiguration("NonExistentSection");
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*NonExistentSection*not found*");
    }

    [Fact]
    public void AddGithubConfiguration_WithSectionNameAndHttpClient_ThrowsWhenSectionNotFound()
    {
        var configurationManager = new ConfigurationManager();
        var httpClient = new HttpClient();

        var act = () => configurationManager.AddGithubConfiguration("NonExistentSection", httpClient);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*NonExistentSection*not found*");
    }

    [Fact]
    public void AddGithubConfiguration_WithSectionName_LoadsFromConfiguration()
    {
        var initialConfig = new Dictionary<string, string?>
        {
            ["GithubConfiguration:AccessToken"] = "test-token",
            ["GithubConfiguration:Owner"] = "test-owner",
            ["GithubConfiguration:Repo"] = "test-repo"
        };

        var configurationManager = new ConfigurationManager();
        configurationManager.AddInMemoryCollection(initialConfig);

        var result = configurationManager.AddGithubConfiguration("GithubConfiguration");

        result.Should().NotBeNull();
    }

    [Fact]
    public void AddGithubConfiguration_WithSectionNameAndHttpClient_LoadsFromConfiguration()
    {
        var initialConfig = new Dictionary<string, string?>
        {
            ["GithubConfiguration:AccessToken"] = "test-token",
            ["GithubConfiguration:Owner"] = "test-owner",
            ["GithubConfiguration:Repo"] = "test-repo"
        };

        var configurationManager = new ConfigurationManager();
        configurationManager.AddInMemoryCollection(initialConfig);
        var httpClient = new HttpClient();

        var result = configurationManager.AddGithubConfiguration("GithubConfiguration", httpClient);

        result.Should().NotBeNull();
    }

    [Fact]
    public void AddGithubConfiguration_WithCustomSectionName_LoadsFromConfiguration()
    {
        var initialConfig = new Dictionary<string, string?>
        {
            ["CustomSection:AccessToken"] = "test-token",
            ["CustomSection:Owner"] = "test-owner",
            ["CustomSection:Repo"] = "test-repo"
        };

        var configurationManager = new ConfigurationManager();
        configurationManager.AddInMemoryCollection(initialConfig);

        var result = configurationManager.AddGithubConfiguration("CustomSection");

        result.Should().NotBeNull();
    }

    [Fact]
    public void GetGithubConfigurationProvider_WithProvider_ReturnsProvider()
    {
        var configurationManager = new ConfigurationManager();
        configurationManager.AddGithubConfiguration(_validOptions);
        var configurationRoot = (IConfigurationRoot)configurationManager;

        var provider = configurationRoot.GetGithubConfigurationProvider();

        provider.Should().NotBeNull();
        provider.Should().BeOfType<GithubConfigurationProvider>();
    }

    [Fact]
    public void GetGithubConfigurationProvider_WithoutProvider_ReturnsNull()
    {
        var configurationManager = new ConfigurationManager();
        configurationManager.AddInMemoryCollection(new Dictionary<string, string?>());
        var configurationRoot = (IConfigurationRoot)configurationManager;

        var provider = configurationRoot.GetGithubConfigurationProvider();

        provider.Should().BeNull();
    }

    [Fact]
    public void CreateWebhookHandler_WithProvider_ReturnsHandler()
    {
        var configurationManager = new ConfigurationManager();
        configurationManager.AddGithubConfiguration(_validOptions);
        var configurationRoot = (IConfigurationRoot)configurationManager;

        var handler = configurationRoot.CreateWebhookHandler(_validOptions);

        handler.Should().NotBeNull();
        handler.Should().BeOfType<GithubWebhookHandler>();
    }

    [Fact]
    public void CreateWebhookHandler_WithoutProvider_ReturnsNull()
    {
        var configurationManager = new ConfigurationManager();
        configurationManager.AddInMemoryCollection(new Dictionary<string, string?>());
        var configurationRoot = (IConfigurationRoot)configurationManager;

        var handler = configurationRoot.CreateWebhookHandler(_validOptions);

        handler.Should().BeNull();
    }

    [Fact]
    public void AddGithubConfiguration_PreservesOtherProviders()
    {
        var initialConfig = new Dictionary<string, string?>
        {
            ["ExistingKey"] = "ExistingValue"
        };

        var configurationManager = new ConfigurationManager();
        configurationManager.AddInMemoryCollection(initialConfig);
        configurationManager.AddGithubConfiguration(_validOptions);

        configurationManager["ExistingKey"].Should().Be("ExistingValue");
    }

    [Fact]
    public void AddGithubConfiguration_WithEncryption_LoadsConfiguration()
    {
        var options = new GithubConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo",
            EnableEncryption = true,
            EncryptionKey = "test-encryption-key"
        };

        var configurationManager = new ConfigurationManager();

        var result = configurationManager.AddGithubConfiguration(options);

        result.Should().NotBeNull();
    }

    [Fact]
    public void AddGithubConfiguration_WithBranch_LoadsConfiguration()
    {
        var options = new GithubConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo",
            Branch = "develop"
        };

        var configurationManager = new ConfigurationManager();

        var result = configurationManager.AddGithubConfiguration(options);

        result.Should().NotBeNull();
    }

    [Fact]
    public void AddGithubConfiguration_WithCustomPath_LoadsConfiguration()
    {
        var options = new GithubConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo",
            ConfigPath = "myconfig",
            ConfigFileName = "settings.json"
        };

        var configurationManager = new ConfigurationManager();

        var result = configurationManager.AddGithubConfiguration(options);

        result.Should().NotBeNull();
    }
}
