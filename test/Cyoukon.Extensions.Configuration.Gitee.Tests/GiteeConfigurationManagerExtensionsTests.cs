using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Cyoukon.Extensions.Configuration.Gitee.Tests;

public class GiteeConfigurationManagerExtensionsTests
{
    private readonly GiteeConfigurationOptions _validOptions;

    public GiteeConfigurationManagerExtensionsTests()
    {
        _validOptions = new GiteeConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo"
        };
    }

    [Fact]
    public void AddGiteeConfiguration_WithAction_ReturnsConfigurationManager()
    {
        var configurationManager = new ConfigurationManager();

        var result = configurationManager.AddGiteeConfiguration(options =>
        {
            options.AccessToken = "test-token";
            options.Owner = "test-owner";
            options.Repo = "test-repo";
        });

        result.Should().NotBeNull();
        result.Should().Be(configurationManager);
    }

    [Fact]
    public void AddGiteeConfiguration_WithActionAndHttpClient_ReturnsConfigurationManager()
    {
        var configurationManager = new ConfigurationManager();
        var httpClient = new HttpClient();

        var result = configurationManager.AddGiteeConfiguration(options =>
        {
            options.AccessToken = "test-token";
            options.Owner = "test-owner";
            options.Repo = "test-repo";
        }, httpClient);

        result.Should().NotBeNull();
        result.Should().Be(configurationManager);
    }

    [Fact]
    public void AddGiteeConfiguration_WithOptions_ReturnsConfigurationManager()
    {
        var configurationManager = new ConfigurationManager();

        var result = configurationManager.AddGiteeConfiguration(_validOptions);

        result.Should().NotBeNull();
        result.Should().Be(configurationManager);
    }

    [Fact]
    public void AddGiteeConfiguration_WithOptionsAndHttpClient_ReturnsConfigurationManager()
    {
        var configurationManager = new ConfigurationManager();
        var httpClient = new HttpClient();

        var result = configurationManager.AddGiteeConfiguration(_validOptions, httpClient);

        result.Should().NotBeNull();
        result.Should().Be(configurationManager);
    }

    [Fact]
    public void AddGiteeConfiguration_WithInvalidOptions_ThrowsArgumentException()
    {
        var configurationManager = new ConfigurationManager();
        var invalidOptions = new GiteeConfigurationOptions
        {
            AccessToken = "",
            Owner = "test-owner",
            Repo = "test-repo"
        };

        var act = () => configurationManager.AddGiteeConfiguration(invalidOptions);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddGiteeConfiguration_WithInvalidActionOptions_ThrowsArgumentException()
    {
        var configurationManager = new ConfigurationManager();

        var act = () => configurationManager.AddGiteeConfiguration(options =>
        {
            options.AccessToken = "";
            options.Owner = "test-owner";
            options.Repo = "test-repo";
        });
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddGiteeConfiguration_WithSectionName_ThrowsWhenSectionNotFound()
    {
        var configurationManager = new ConfigurationManager();

        var act = () => configurationManager.AddGiteeConfiguration("NonExistentSection");
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*NonExistentSection*not found*");
    }

    [Fact]
    public void AddGiteeConfiguration_WithSectionNameAndHttpClient_ThrowsWhenSectionNotFound()
    {
        var configurationManager = new ConfigurationManager();
        var httpClient = new HttpClient();

        var act = () => configurationManager.AddGiteeConfiguration("NonExistentSection", httpClient);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*NonExistentSection*not found*");
    }

    [Fact]
    public void AddGiteeConfiguration_WithSectionName_LoadsFromConfiguration()
    {
        var initialConfig = new Dictionary<string, string?>
        {
            ["GiteeConfiguration:AccessToken"] = "test-token",
            ["GiteeConfiguration:Owner"] = "test-owner",
            ["GiteeConfiguration:Repo"] = "test-repo"
        };

        var configurationManager = new ConfigurationManager();
        configurationManager.AddInMemoryCollection(initialConfig);

        var result = configurationManager.AddGiteeConfiguration("GiteeConfiguration");

        result.Should().NotBeNull();
    }

    [Fact]
    public void AddGiteeConfiguration_WithSectionNameAndHttpClient_LoadsFromConfiguration()
    {
        var initialConfig = new Dictionary<string, string?>
        {
            ["GiteeConfiguration:AccessToken"] = "test-token",
            ["GiteeConfiguration:Owner"] = "test-owner",
            ["GiteeConfiguration:Repo"] = "test-repo"
        };

        var configurationManager = new ConfigurationManager();
        configurationManager.AddInMemoryCollection(initialConfig);
        var httpClient = new HttpClient();

        var result = configurationManager.AddGiteeConfiguration("GiteeConfiguration", httpClient);

        result.Should().NotBeNull();
    }

    [Fact]
    public void AddGiteeConfiguration_WithCustomSectionName_LoadsFromConfiguration()
    {
        var initialConfig = new Dictionary<string, string?>
        {
            ["CustomSection:AccessToken"] = "test-token",
            ["CustomSection:Owner"] = "test-owner",
            ["CustomSection:Repo"] = "test-repo"
        };

        var configurationManager = new ConfigurationManager();
        configurationManager.AddInMemoryCollection(initialConfig);

        var result = configurationManager.AddGiteeConfiguration("CustomSection");

        result.Should().NotBeNull();
    }

    [Fact]
    public void GetGiteeConfigurationProvider_WithProvider_ReturnsProvider()
    {
        var configurationManager = new ConfigurationManager();
        configurationManager.AddGiteeConfiguration(_validOptions);
        var configurationRoot = (IConfigurationRoot)configurationManager;

        var provider = configurationRoot.GetGiteeConfigurationProvider();

        provider.Should().NotBeNull();
        provider.Should().BeOfType<GiteeConfigurationProvider>();
    }

    [Fact]
    public void GetGiteeConfigurationProvider_WithoutProvider_ReturnsNull()
    {
        var configurationManager = new ConfigurationManager();
        configurationManager.AddInMemoryCollection(new Dictionary<string, string?>());
        var configurationRoot = (IConfigurationRoot)configurationManager;

        var provider = configurationRoot.GetGiteeConfigurationProvider();

        provider.Should().BeNull();
    }

    [Fact]
    public void CreateWebhookHandler_WithProvider_ReturnsHandler()
    {
        var configurationManager = new ConfigurationManager();
        configurationManager.AddGiteeConfiguration(_validOptions);
        var configurationRoot = (IConfigurationRoot)configurationManager;

        var handler = configurationRoot.CreateWebhookHandler(_validOptions);

        handler.Should().NotBeNull();
        handler.Should().BeOfType<GiteeWebhookHandler>();
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
    public void AddGiteeConfiguration_PreservesOtherProviders()
    {
        var initialConfig = new Dictionary<string, string?>
        {
            ["ExistingKey"] = "ExistingValue"
        };

        var configurationManager = new ConfigurationManager();
        configurationManager.AddInMemoryCollection(initialConfig);
        configurationManager.AddGiteeConfiguration(_validOptions);

        configurationManager["ExistingKey"].Should().Be("ExistingValue");
    }

    [Fact]
    public void AddGiteeConfiguration_WithEncryption_LoadsConfiguration()
    {
        var options = new GiteeConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo",
            EnableEncryption = true,
            EncryptionKey = "test-encryption-key"
        };

        var configurationManager = new ConfigurationManager();

        var result = configurationManager.AddGiteeConfiguration(options);

        result.Should().NotBeNull();
    }

    [Fact]
    public void AddGiteeConfiguration_WithBranch_LoadsConfiguration()
    {
        var options = new GiteeConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo",
            Branch = "develop"
        };

        var configurationManager = new ConfigurationManager();

        var result = configurationManager.AddGiteeConfiguration(options);

        result.Should().NotBeNull();
    }

    [Fact]
    public void AddGiteeConfiguration_WithCustomPath_LoadsConfiguration()
    {
        var options = new GiteeConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo",
            ConfigPath = "myconfig",
            ConfigFileName = "settings.json"
        };

        var configurationManager = new ConfigurationManager();

        var result = configurationManager.AddGiteeConfiguration(options);

        result.Should().NotBeNull();
    }
}
