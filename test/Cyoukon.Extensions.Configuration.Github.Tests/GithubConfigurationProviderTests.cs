using Cyoukon.Extensions.Configuration.Abstractions.Services;
using Cyoukon.Extensions.Configuration.Github.Models;
using Cyoukon.Extensions.Configuration.Github.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace Cyoukon.Extensions.Configuration.Github.Tests;

public class GithubConfigurationProviderTests
{
    private readonly Mock<IGithubApiClient> _mockApiClient;
    private readonly GithubConfigurationOptions _options;

    public GithubConfigurationProviderTests()
    {
        _mockApiClient = new Mock<IGithubApiClient>();
        _options = new GithubConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo"
        };
    }

    [Fact]
    public void Constructor_WithValidParameters_InitializesCorrectly()
    {
        var provider = new GithubConfigurationProvider(_mockApiClient.Object, _options, null);
        provider.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithEncryptionService_InitializesCorrectly()
    {
        var encryptionService = new AesEncryptionService("test-key");
        var options = new GithubConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo",
            EnableEncryption = true,
            EncryptionKey = "test-key"
        };

        var provider = new GithubConfigurationProvider(_mockApiClient.Object, options, encryptionService);
        provider.Should().NotBeNull();
    }

    [Fact]
    public void Load_WithValidFileContent_LoadsConfiguration()
    {
        var jsonContent = "{\"key1\": \"value1\", \"key2\": \"value2\"}";
        var base64Content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonContent));
        var fileContent = new GithubFileContent
        {
            Content = base64Content,
            Sha = "test-sha",
            Path = "config/appsettings.json"
        };

        _mockApiClient
            .Setup(x => x.GetFileContentAsync(It.IsAny<string>(), default))
            .ReturnsAsync(fileContent);

        var provider = new GithubConfigurationProvider(_mockApiClient.Object, _options, null);
        provider.Load();

        provider.TryGet("key1", out var value1).Should().BeTrue();
        value1.Should().Be("value1");
        provider.TryGet("key2", out var value2).Should().BeTrue();
        value2.Should().Be("value2");
    }

    [Fact]
    public void Load_WithNullFileContent_SetsEmptyData()
    {
        _mockApiClient
            .Setup(x => x.GetFileContentAsync(It.IsAny<string>(), default))
            .ReturnsAsync((GithubFileContent?)null);

        var provider = new GithubConfigurationProvider(_mockApiClient.Object, _options, null);
        provider.Load();

        provider.TryGet("any-key", out _).Should().BeFalse();
    }

    [Fact]
    public void Load_WithFileContentNullContent_SetsEmptyData()
    {
        var fileContent = new GithubFileContent
        {
            Content = null,
            Sha = "test-sha"
        };

        _mockApiClient
            .Setup(x => x.GetFileContentAsync(It.IsAny<string>(), default))
            .ReturnsAsync(fileContent);

        var provider = new GithubConfigurationProvider(_mockApiClient.Object, _options, null);
        provider.Load();

        provider.TryGet("any-key", out _).Should().BeFalse();
    }

    [Fact]
    public void Load_WithException_SetsError()
    {
        _mockApiClient
            .Setup(x => x.GetFileContentAsync(It.IsAny<string>(), default))
            .ThrowsAsync(new Exception("Test error"));

        var provider = new GithubConfigurationProvider(_mockApiClient.Object, _options, null);
        provider.Load();

        provider.TryGet("GitConfiguration:Error", out var error).Should().BeTrue();
        error.Should().Contain("Failed to load configuration from GitHub");
    }

    [Fact]
    public void Load_WithEncryptionEnabled_DecryptsContent()
    {
        var encryptionService = new AesEncryptionService("test-key");
        var jsonContent = "{\"key1\": \"value1\"}";
        var encryptedContent = encryptionService.Encrypt(jsonContent);
        var base64Content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(encryptedContent));
        var fileContent = new GithubFileContent
        {
            Content = base64Content,
            Sha = "test-sha"
        };

        var options = new GithubConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo",
            EnableEncryption = true,
            EncryptionKey = "test-key"
        };

        _mockApiClient
            .Setup(x => x.GetFileContentAsync(It.IsAny<string>(), default))
            .ReturnsAsync(fileContent);

        var provider = new GithubConfigurationProvider(_mockApiClient.Object, options, encryptionService);
        provider.Load();

        provider.TryGet("key1", out var value).Should().BeTrue();
        value.Should().Be("value1");
    }

    [Fact]
    public async Task SaveAsync_WithValidData_ReturnsTrue()
    {
        var data = new Dictionary<string, string?> { ["key1"] = "value1" };
        var commitResponse = new GithubCommitResponse
        {
            Content = new GithubFileContent { Sha = "new-sha" }
        };

        _mockApiClient
            .Setup(x => x.CreateOrUpdateFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), default))
            .ReturnsAsync(commitResponse);

        var provider = new GithubConfigurationProvider(_mockApiClient.Object, _options, null);
        var result = await provider.SaveAsync(data);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_WithNullResponse_ReturnsFalse()
    {
        var data = new Dictionary<string, string?> { ["key1"] = "value1" };

        _mockApiClient
            .Setup(x => x.CreateOrUpdateFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), default))
            .ReturnsAsync((GithubCommitResponse?)null);

        var provider = new GithubConfigurationProvider(_mockApiClient.Object, _options, null);
        var result = await provider.SaveAsync(data);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SaveAsync_WithNullContent_ReturnsFalse()
    {
        var data = new Dictionary<string, string?> { ["key1"] = "value1" };
        var commitResponse = new GithubCommitResponse
        {
            Content = null
        };

        _mockApiClient
            .Setup(x => x.CreateOrUpdateFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), default))
            .ReturnsAsync(commitResponse);

        var provider = new GithubConfigurationProvider(_mockApiClient.Object, _options, null);
        var result = await provider.SaveAsync(data);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SaveAsync_WithException_ReturnsFalse()
    {
        var data = new Dictionary<string, string?> { ["key1"] = "value1" };

        _mockApiClient
            .Setup(x => x.CreateOrUpdateFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), default))
            .ThrowsAsync(new Exception("Test error"));

        var provider = new GithubConfigurationProvider(_mockApiClient.Object, _options, null);
        var result = await provider.SaveAsync(data);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SaveAsync_WithEncryptionEnabled_EncryptsContent()
    {
        var encryptionService = new AesEncryptionService("test-key");
        var data = new Dictionary<string, string?> { ["key1"] = "value1" };
        var commitResponse = new GithubCommitResponse
        {
            Content = new GithubFileContent { Sha = "new-sha" }
        };

        var options = new GithubConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo",
            EnableEncryption = true,
            EncryptionKey = "test-key"
        };

        string? capturedContent = null;
        _mockApiClient
            .Setup(x => x.CreateOrUpdateFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), default))
            .Callback<string, string, string, string?, CancellationToken>((_, content, _, _, _) => capturedContent = content)
            .ReturnsAsync(commitResponse);

        var provider = new GithubConfigurationProvider(_mockApiClient.Object, options, encryptionService);
        await provider.SaveAsync(data);

        capturedContent.Should().NotBeNull();
        var decrypted = encryptionService.Decrypt(capturedContent!);
        decrypted.Should().Contain("key1");
    }

    [Fact]
    public async Task SaveAsync_UpdatesSha()
    {
        var data = new Dictionary<string, string?> { ["key1"] = "value1" };
        var commitResponse = new GithubCommitResponse
        {
            Content = new GithubFileContent { Sha = "new-sha" }
        };

        _mockApiClient
            .Setup(x => x.CreateOrUpdateFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), default))
            .ReturnsAsync(commitResponse);

        var provider = new GithubConfigurationProvider(_mockApiClient.Object, _options, null);
        await provider.SaveAsync(data);

        _mockApiClient.Verify(x => x.CreateOrUpdateFileAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string?>(),
            default), Times.Once);
    }

    [Fact]
    public async Task SetAsync_AddsNewKeyAndSaves()
    {
        var commitResponse = new GithubCommitResponse
        {
            Content = new GithubFileContent { Sha = "new-sha" }
        };

        var jsonContent = "{\"existing\": \"value\"}";
        var base64Content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonContent));
        var fileContent = new GithubFileContent
        {
            Content = base64Content,
            Sha = "test-sha"
        };

        _mockApiClient
            .Setup(x => x.GetFileContentAsync(It.IsAny<string>(), default))
            .ReturnsAsync(fileContent);
        _mockApiClient
            .Setup(x => x.CreateOrUpdateFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), default))
            .ReturnsAsync(commitResponse);

        var provider = new GithubConfigurationProvider(_mockApiClient.Object, _options, null);
        provider.Load();

        var result = await provider.SetAsync("newKey", "newValue");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveAsync_RemovesKeyAndSaves()
    {
        var commitResponse = new GithubCommitResponse
        {
            Content = new GithubFileContent { Sha = "new-sha" }
        };

        var jsonContent = "{\"key1\": \"value1\", \"key2\": \"value2\"}";
        var base64Content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonContent));
        var fileContent = new GithubFileContent
        {
            Content = base64Content,
            Sha = "test-sha"
        };

        _mockApiClient
            .Setup(x => x.GetFileContentAsync(It.IsAny<string>(), default))
            .ReturnsAsync(fileContent);
        _mockApiClient
            .Setup(x => x.CreateOrUpdateFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), default))
            .ReturnsAsync(commitResponse);

        var provider = new GithubConfigurationProvider(_mockApiClient.Object, _options, null);
        provider.Load();

        var result = await provider.RemoveAsync("key1");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ReloadAsync_ReloadsConfiguration()
    {
        var jsonContent = "{\"key1\": \"value1\"}";
        var base64Content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonContent));
        var fileContent = new GithubFileContent
        {
            Content = base64Content,
            Sha = "test-sha"
        };

        _mockApiClient
            .Setup(x => x.GetFileContentAsync(It.IsAny<string>(), default))
            .ReturnsAsync(fileContent);

        var provider = new GithubConfigurationProvider(_mockApiClient.Object, _options, null);
        await provider.ReloadAsync();

        provider.TryGet("key1", out var value).Should().BeTrue();
        value.Should().Be("value1");
    }

    [Fact]
    public async Task OnWebhookReceivedAsync_ReloadsConfiguration()
    {
        var jsonContent = "{\"key1\": \"value1\"}";
        var base64Content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonContent));
        var fileContent = new GithubFileContent
        {
            Content = base64Content,
            Sha = "test-sha"
        };

        _mockApiClient
            .Setup(x => x.GetFileContentAsync(It.IsAny<string>(), default))
            .ReturnsAsync(fileContent);

        var provider = new GithubConfigurationProvider(_mockApiClient.Object, _options, null);
        await provider.OnWebhookReceivedAsync();

        provider.TryGet("key1", out var value).Should().BeTrue();
        value.Should().Be("value1");
    }

    [Fact]
    public void Load_WithNestedJson_ParsesCorrectly()
    {
        var jsonContent = "{\"parent\": {\"child\": \"value\"}}";
        var base64Content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonContent));
        var fileContent = new GithubFileContent
        {
            Content = base64Content,
            Sha = "test-sha"
        };

        _mockApiClient
            .Setup(x => x.GetFileContentAsync(It.IsAny<string>(), default))
            .ReturnsAsync(fileContent);

        var provider = new GithubConfigurationProvider(_mockApiClient.Object, _options, null);
        provider.Load();

        provider.TryGet("parent:child", out var value).Should().BeTrue();
        value.Should().Be("value");
    }

    [Fact]
    public void Load_WithArrayJson_ParsesCorrectly()
    {
        var jsonContent = "{\"items\": [\"a\", \"b\", \"c\"]}";
        var base64Content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonContent));
        var fileContent = new GithubFileContent
        {
            Content = base64Content,
            Sha = "test-sha"
        };

        _mockApiClient
            .Setup(x => x.GetFileContentAsync(It.IsAny<string>(), default))
            .ReturnsAsync(fileContent);

        var provider = new GithubConfigurationProvider(_mockApiClient.Object, _options, null);
        provider.Load();

        provider.TryGet("items:0", out var value0).Should().BeTrue();
        value0.Should().Be("a");
        provider.TryGet("items:1", out var value1).Should().BeTrue();
        value1.Should().Be("b");
        provider.TryGet("items:2", out var value2).Should().BeTrue();
        value2.Should().Be("c");
    }

    [Fact]
    public void Load_WithCustomPath_UsesCorrectPath()
    {
        var options = new GithubConfigurationOptions
        {
            AccessToken = "test-token",
            Owner = "test-owner",
            Repo = "test-repo",
            ConfigPath = "myconfig",
            ConfigFileName = "settings.json"
        };

        var jsonContent = "{\"key1\": \"value1\"}";
        var base64Content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonContent));
        var fileContent = new GithubFileContent
        {
            Content = base64Content,
            Sha = "test-sha"
        };

        _mockApiClient
            .Setup(x => x.GetFileContentAsync("myconfig/settings.json", default))
            .ReturnsAsync(fileContent);

        var provider = new GithubConfigurationProvider(_mockApiClient.Object, options, null);
        provider.Load();

        _mockApiClient.Verify(x => x.GetFileContentAsync("myconfig/settings.json", default), Times.Once);
    }

    [Fact]
    public void Dispose_DisposesCorrectly()
    {
        var provider = new GithubConfigurationProvider(_mockApiClient.Object, _options, null);
        var act = () => provider.Dispose();
        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        var provider = new GithubConfigurationProvider(_mockApiClient.Object, _options, null);
        provider.Dispose();
        var act = () => provider.Dispose();
        act.Should().NotThrow();
    }
}
