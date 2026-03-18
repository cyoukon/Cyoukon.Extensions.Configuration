using FluentAssertions;
using Xunit;

namespace Cyoukon.Extensions.Configuration.Github.Tests;

public class GithubConfigurationOptionsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var options = new GithubConfigurationOptions();

        options.ApiBaseUrl.Should().Be("https://api.github.com");
        options.ApiVersion.Should().Be("2022-11-28");
        options.Branch.Should().Be("main");
        options.ConfigPath.Should().Be("config");
        options.ConfigFileName.Should().Be("appsettings.json");
        options.EnableEncryption.Should().BeFalse();
        options.EnableReload.Should().BeFalse();
        options.ReloadInterval.Should().Be(TimeSpan.FromMinutes(5));
        options.EncryptionKey.Should().BeNull();
        options.WebhookSecret.Should().BeNull();
    }

    [Fact]
    public void Validate_WithValidOptions_DoesNotThrow()
    {
        var options = new GithubConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo"
        };

        var act = () => options.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithEmptyAccessToken_ThrowsArgumentException()
    {
        var options = new GithubConfigurationOptions
        {
            AccessToken = "",
            Owner = "test-owner",
            Repo = "test-repo"
        };

        var act = () => options.Validate();
        act.Should().Throw<ArgumentException>()
           .WithMessage("*AccessToken*");
    }

    [Fact]
    public void Validate_WithWhitespaceAccessToken_ThrowsArgumentException()
    {
        var options = new GithubConfigurationOptions
        {
            AccessToken = "   ",
            Owner = "test-owner",
            Repo = "test-repo"
        };

        var act = () => options.Validate();
        act.Should().Throw<ArgumentException>()
           .WithMessage("*AccessToken*");
    }

    [Fact]
    public void Validate_WithEmptyOwner_ThrowsArgumentException()
    {
        var options = new GithubConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "",
            Repo = "test-repo"
        };

        var act = () => options.Validate();
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Owner*");
    }

    [Fact]
    public void Validate_WithEmptyRepo_ThrowsArgumentException()
    {
        var options = new GithubConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = ""
        };

        var act = () => options.Validate();
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Repo*");
    }

    [Fact]
    public void Validate_WithEncryptionEnabledButNoKey_ThrowsArgumentException()
    {
        var options = new GithubConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo",
            EnableEncryption = true,
            EncryptionKey = null
        };

        var act = () => options.Validate();
        act.Should().Throw<ArgumentException>()
           .WithMessage("*EncryptionKey*");
    }

    [Fact]
    public void Validate_WithEncryptionEnabledAndEmptyKey_ThrowsArgumentException()
    {
        var options = new GithubConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo",
            EnableEncryption = true,
            EncryptionKey = ""
        };

        var act = () => options.Validate();
        act.Should().Throw<ArgumentException>()
           .WithMessage("*EncryptionKey*");
    }

    [Fact]
    public void Validate_WithEncryptionEnabledAndValidKey_DoesNotThrow()
    {
        var options = new GithubConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo",
            EnableEncryption = true,
            EncryptionKey = "encryption-key"
        };

        var act = () => options.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void GetFilePath_WithDefaultValues_ReturnsCorrectPath()
    {
        var options = new GithubConfigurationOptions();
        var result = options.GetFilePath();
        result.Should().Be("config/appsettings.json");
    }

    [Fact]
    public void GetFilePath_WithCustomPath_ReturnsCorrectPath()
    {
        var options = new GithubConfigurationOptions
        {
            ConfigPath = "myconfigs",
            ConfigFileName = "settings.json"
        };
        var result = options.GetFilePath();
        result.Should().Be("myconfigs/settings.json");
    }

    [Fact]
    public void GetFilePath_WithLeadingSlash_TrimsSlash()
    {
        var options = new GithubConfigurationOptions
        {
            ConfigPath = "/config",
            ConfigFileName = "appsettings.json"
        };
        var result = options.GetFilePath();
        result.Should().Be("config/appsettings.json");
    }

    [Fact]
    public void ApiBaseUrl_CanBeCustomized()
    {
        var options = new GithubConfigurationOptions
        {
            ApiBaseUrl = "https://custom.github.com/api"
        };

        options.ApiBaseUrl.Should().Be("https://custom.github.com/api");
    }

    [Fact]
    public void ApiVersion_CanBeCustomized()
    {
        var options = new GithubConfigurationOptions
        {
            ApiVersion = "2023-01-01"
        };

        options.ApiVersion.Should().Be("2023-01-01");
    }

    [Fact]
    public void Branch_CanBeCustomized()
    {
        var options = new GithubConfigurationOptions
        {
            Branch = "develop"
        };

        options.Branch.Should().Be("develop");
    }

    [Fact]
    public void WebhookSecret_CanBeSet()
    {
        var options = new GithubConfigurationOptions
        {
            WebhookSecret = "my-webhook-secret"
        };

        options.WebhookSecret.Should().Be("my-webhook-secret");
    }

    [Fact]
    public void EnableReload_CanBeEnabled()
    {
        var options = new GithubConfigurationOptions
        {
            EnableReload = true
        };

        options.EnableReload.Should().BeTrue();
    }

    [Fact]
    public void ReloadInterval_CanBeCustomized()
    {
        var options = new GithubConfigurationOptions
        {
            ReloadInterval = TimeSpan.FromMinutes(10)
        };

        options.ReloadInterval.Should().Be(TimeSpan.FromMinutes(10));
    }
}
