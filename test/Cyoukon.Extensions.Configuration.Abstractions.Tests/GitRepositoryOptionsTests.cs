using FluentAssertions;
using Xunit;

namespace Cyoukon.Extensions.Configuration.Abstractions.Tests;

public class GitRepositoryOptionsTests
{
    private class TestGitRepositoryOptions : GitRepositoryOptions { }

    [Fact]
    public void GetFilePath_WithDefaultValues_ReturnsCorrectPath()
    {
        var options = new TestGitRepositoryOptions();
        var result = options.GetFilePath();
        result.Should().Be("config/appsettings.json");
    }

    [Fact]
    public void GetFilePath_WithCustomPath_ReturnsCorrectPath()
    {
        var options = new TestGitRepositoryOptions
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
        var options = new TestGitRepositoryOptions
        {
            ConfigPath = "/config",
            ConfigFileName = "appsettings.json"
        };
        var result = options.GetFilePath();
        result.Should().Be("config/appsettings.json");
    }

    [Fact]
    public void Validate_WithValidOptions_DoesNotThrow()
    {
        var options = new TestGitRepositoryOptions
        {
            AccessToken = "token",
            Owner = "owner",
            Repo = "repo"
        };
        
        var act = () => options.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithEmptyAccessToken_ThrowsArgumentException()
    {
        var options = new TestGitRepositoryOptions
        {
            AccessToken = "",
            Owner = "owner",
            Repo = "repo"
        };
        
        var act = () => options.Validate();
        act.Should().Throw<ArgumentException>()
           .WithMessage("*AccessToken*");
    }

    [Fact]
    public void Validate_WithWhitespaceAccessToken_ThrowsArgumentException()
    {
        var options = new TestGitRepositoryOptions
        {
            AccessToken = "   ",
            Owner = "owner",
            Repo = "repo"
        };
        
        var act = () => options.Validate();
        act.Should().Throw<ArgumentException>()
           .WithMessage("*AccessToken*");
    }

    [Fact]
    public void Validate_WithEmptyOwner_ThrowsArgumentException()
    {
        var options = new TestGitRepositoryOptions
        {
            AccessToken = "token",
            Owner = "",
            Repo = "repo"
        };
        
        var act = () => options.Validate();
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Owner*");
    }

    [Fact]
    public void Validate_WithEmptyRepo_ThrowsArgumentException()
    {
        var options = new TestGitRepositoryOptions
        {
            AccessToken = "token",
            Owner = "owner",
            Repo = ""
        };
        
        var act = () => options.Validate();
        act.Should().Throw<ArgumentException>()
           .WithMessage("*Repo*");
    }

    [Fact]
    public void Validate_WithEncryptionEnabledButNoKey_ThrowsArgumentException()
    {
        var options = new TestGitRepositoryOptions
        {
            AccessToken = "token",
            Owner = "owner",
            Repo = "repo",
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
        var options = new TestGitRepositoryOptions
        {
            AccessToken = "token",
            Owner = "owner",
            Repo = "repo",
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
        var options = new TestGitRepositoryOptions
        {
            AccessToken = "token",
            Owner = "owner",
            Repo = "repo",
            EnableEncryption = true,
            EncryptionKey = "encryption-key"
        };
        
        var act = () => options.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var options = new TestGitRepositoryOptions();
        
        options.Branch.Should().Be("main");
        options.ConfigPath.Should().Be("config");
        options.ConfigFileName.Should().Be("appsettings.json");
        options.EnableEncryption.Should().BeFalse();
        options.EnableReload.Should().BeFalse();
        options.ReloadInterval.Should().Be(TimeSpan.FromMinutes(5));
        options.EncryptionKey.Should().BeNull();
        options.WebhookSecret.Should().BeNull();
    }
}
