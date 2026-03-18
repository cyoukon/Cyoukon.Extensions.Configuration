using Cyoukon.Extensions.Configuration.Abstractions.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Cyoukon.Extensions.Configuration.Abstractions.Tests;

public class GitConfigurationProviderBaseTests
{
    private class TestGitRepositoryOptions : GitRepositoryOptions { }

    private class TestConfigurationProvider : GitConfigurationProviderBase<TestGitRepositoryOptions>
    {
        private readonly Func<Task>? _loadAction;
        private readonly Func<Dictionary<string, string?>, string, Task<bool>>? _saveAction;

        public TestConfigurationProvider(
            TestGitRepositoryOptions options,
            IEncryptionService? encryptionService,
            Func<Task>? loadAction = null,
            Func<Dictionary<string, string?>, Task<bool>>? saveAction = null)
            : base(options, encryptionService)
        {
            _loadAction = loadAction;
            if (saveAction != null)
            {
                _saveAction = (data, message) => saveAction(data);
            }
        }

        public TestConfigurationProvider(
            TestGitRepositoryOptions options,
            IEncryptionService? encryptionService,
            Func<Task>? loadAction,
            Func<Dictionary<string, string?>, string, Task<bool>>? saveAction)
            : base(options, encryptionService)
        {
            _loadAction = loadAction;
            _saveAction = saveAction;
        }

        protected override Task LoadAsync()
        {
            return _loadAction?.Invoke() ?? Task.CompletedTask;
        }

        public override Task<bool> SaveAsync(Dictionary<string, string?> data, string commitMessage = "Update configuration")
        {
            return _saveAction?.Invoke(data, commitMessage) ?? Task.FromResult(true);
        }

        public void ExposeSetData(Dictionary<string, string?> data, bool fireReload)
        {
            SetData(data, fireReload);
        }

        public void ExposeSetError(string message)
        {
            SetError(message);
        }

        public string ExposeProcessContentBeforeLoad(string content)
        {
            return ProcessContentBeforeLoad(content);
        }

        public string ExposeProcessContentBeforeSave(string content)
        {
            return ProcessContentBeforeSave(content);
        }
    }

    private readonly TestGitRepositoryOptions _validOptions;

    public GitConfigurationProviderBaseTests()
    {
        _validOptions = new TestGitRepositoryOptions
        {
            AccessToken = "token",
            Owner = "owner",
            Repo = "repo"
        };
    }

    [Fact]
    public void Constructor_WithValidOptions_InitializesCorrectly()
    {
        var provider = new TestConfigurationProvider(_validOptions, null);
        provider.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithEncryptionService_StoresService()
    {
        var encryptionService = new AesEncryptionService("test-key");
        var options = new TestGitRepositoryOptions
        {
            AccessToken = "token",
            Owner = "owner",
            Repo = "repo"
        };
        
        var provider = new TestConfigurationProvider(options, encryptionService);
        provider.Should().NotBeNull();
    }

    [Fact]
    public void Load_CallsLoadAsync()
    {
        var loadCalled = false;
        var provider = new TestConfigurationProvider(_validOptions, null, () =>
        {
            loadCalled = true;
            return Task.CompletedTask;
        });
        
        provider.Load();
        
        loadCalled.Should().BeTrue();
    }

    [Fact]
    public void SetData_WithFireReloadTrue_TriggersReload()
    {
        var provider = new TestConfigurationProvider(_validOptions, null);
        var reloadTriggered = false;
        provider.GetReloadToken().RegisterChangeCallback(_ => reloadTriggered = true, null);
        
        provider.ExposeSetData(new Dictionary<string, string?> { ["key"] = "value" }, true);
        
        reloadTriggered.Should().BeTrue();
    }

    [Fact]
    public void SetData_WithFireReloadFalse_DoesNotTriggerReload()
    {
        var provider = new TestConfigurationProvider(_validOptions, null);
        var reloadTriggered = false;
        provider.GetReloadToken().RegisterChangeCallback(_ => reloadTriggered = true, null);
        
        provider.ExposeSetData(new Dictionary<string, string?> { ["key"] = "value" }, false);
        
        reloadTriggered.Should().BeFalse();
    }

    [Fact]
    public void SetError_SetsErrorInData()
    {
        var provider = new TestConfigurationProvider(_validOptions, null);
        
        provider.ExposeSetError("Test error message");
        
        provider.TryGet("GitConfiguration:Error", out var error).Should().BeTrue();
        error.Should().Be("Test error message");
    }

    [Fact]
    public void ProcessContentBeforeLoad_WithoutEncryption_ReturnsOriginalContent()
    {
        var provider = new TestConfigurationProvider(_validOptions, null);
        var content = "test content";
        
        var result = provider.ExposeProcessContentBeforeLoad(content);
        
        result.Should().Be(content);
    }

    [Fact]
    public void ProcessContentBeforeLoad_WithEncryption_DecryptsContent()
    {
        var encryptionService = new AesEncryptionService("test-key");
        var options = new TestGitRepositoryOptions
        {
            AccessToken = "token",
            Owner = "owner",
            Repo = "repo",
            EnableEncryption = true,
            EncryptionKey = "test-key"
        };
        var provider = new TestConfigurationProvider(options, encryptionService);
        var originalContent = "test content";
        var encryptedContent = encryptionService.Encrypt(originalContent);
        
        var result = provider.ExposeProcessContentBeforeLoad(encryptedContent);
        
        result.Should().Be(originalContent);
    }

    [Fact]
    public void ProcessContentBeforeSave_WithoutEncryption_ReturnsOriginalContent()
    {
        var provider = new TestConfigurationProvider(_validOptions, null);
        var content = "test content";
        
        var result = provider.ExposeProcessContentBeforeSave(content);
        
        result.Should().Be(content);
    }

    [Fact]
    public void ProcessContentBeforeSave_WithEncryption_EncryptsContent()
    {
        var encryptionService = new AesEncryptionService("test-key");
        var options = new TestGitRepositoryOptions
        {
            AccessToken = "token",
            Owner = "owner",
            Repo = "repo",
            EnableEncryption = true,
            EncryptionKey = "test-key"
        };
        var provider = new TestConfigurationProvider(options, encryptionService);
        var originalContent = "test content";
        
        var result = provider.ExposeProcessContentBeforeSave(originalContent);
        
        result.Should().NotBe(originalContent);
        var decrypted = encryptionService.Decrypt(result);
        decrypted.Should().Be(originalContent);
    }

    [Fact]
    public async Task SetAsync_UpdatesKeyAndSaves()
    {
        var savedData = (Dictionary<string, string?>?)null;
        var provider = new TestConfigurationProvider(_validOptions, null, null, (data, message) =>
        {
            savedData = data;
            return Task.FromResult(true);
        });
        provider.ExposeSetData(new Dictionary<string, string?> { ["existing"] = "value" }, false);
        
        var result = await provider.SetAsync("newKey", "newValue");
        
        result.Should().BeTrue();
        savedData.Should().ContainKey("existing");
        savedData.Should().ContainKey("newKey");
        savedData!["newKey"].Should().Be("newValue");
    }

    [Fact]
    public async Task RemoveAsync_RemovesKeyAndSaves()
    {
        var savedData = (Dictionary<string, string?>?)null;
        var provider = new TestConfigurationProvider(_validOptions, null, null, (data, message) =>
        {
            savedData = data;
            return Task.FromResult(true);
        });
        provider.ExposeSetData(new Dictionary<string, string?> { ["key1"] = "value1", ["key2"] = "value2" }, false);
        
        var result = await provider.RemoveAsync("key1");
        
        result.Should().BeTrue();
        savedData.Should().NotContainKey("key1");
        savedData.Should().ContainKey("key2");
    }

    [Fact]
    public async Task ReloadAsync_CallsLoadAsyncAndTriggersReload()
    {
        var loadCalled = false;
        var provider = new TestConfigurationProvider(_validOptions, null, () =>
        {
            loadCalled = true;
            return Task.CompletedTask;
        });
        var reloadTriggered = false;
        provider.GetReloadToken().RegisterChangeCallback(_ => reloadTriggered = true, null);
        
        await provider.ReloadAsync();
        
        loadCalled.Should().BeTrue();
        reloadTriggered.Should().BeTrue();
    }

    [Fact]
    public async Task OnWebhookReceivedAsync_CallsLoadAsyncAndTriggersReload()
    {
        var loadCalled = false;
        var provider = new TestConfigurationProvider(_validOptions, null, () =>
        {
            loadCalled = true;
            return Task.CompletedTask;
        });
        var reloadTriggered = false;
        provider.GetReloadToken().RegisterChangeCallback(_ => reloadTriggered = true, null);
        
        await provider.OnWebhookReceivedAsync();
        
        loadCalled.Should().BeTrue();
        reloadTriggered.Should().BeTrue();
    }

    [Fact]
    public void Dispose_DisposesResources()
    {
        var provider = new TestConfigurationProvider(_validOptions, null);
        
        var act = () => provider.Dispose();
        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        var provider = new TestConfigurationProvider(_validOptions, null);
        
        provider.Dispose();
        var act = () => provider.Dispose();
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithEnableReload_StartsReloadLoop()
    {
        var options = new TestGitRepositoryOptions
        {
            AccessToken = "token",
            Owner = "owner",
            Repo = "repo",
            EnableReload = true,
            ReloadInterval = TimeSpan.FromMilliseconds(100)
        };

        var provider = new TestConfigurationProvider(options, null);
        provider.Should().NotBeNull();
        provider.Dispose();
    }

    [Fact]
    public void Constructor_WithoutEnableReload_DoesNotStartReloadLoop()
    {
        var options = new TestGitRepositoryOptions
        {
            AccessToken = "token",
            Owner = "owner",
            Repo = "repo",
            EnableReload = false
        };

        var provider = new TestConfigurationProvider(options, null);
        provider.Should().NotBeNull();
    }

    [Fact]
    public async Task SetAsync_WithCustomCommitMessage_PassesCommitMessage()
    {
        var capturedMessage = string.Empty;
        var provider = new TestConfigurationProvider(_validOptions, null, null, (data, message) =>
        {
            capturedMessage = message;
            return Task.FromResult(true);
        });
        provider.ExposeSetData(new Dictionary<string, string?> { ["key"] = "value" }, false);

        await provider.SetAsync("key", "value", "Custom commit message");

        capturedMessage.Should().Be("Custom commit message");
    }

    [Fact]
    public async Task RemoveAsync_WithCustomCommitMessage_PassesCommitMessage()
    {
        var capturedMessage = string.Empty;
        var provider = new TestConfigurationProvider(_validOptions, null, null, (data, message) =>
        {
            capturedMessage = message;
            return Task.FromResult(true);
        });
        provider.ExposeSetData(new Dictionary<string, string?> { ["key"] = "value" }, false);

        await provider.RemoveAsync("key", "Custom remove message");

        capturedMessage.Should().Be("Custom remove message");
    }

    [Fact]
    public async Task SaveAsync_WithCustomCommitMessage_PassesCommitMessage()
    {
        var capturedMessage = string.Empty;
        var provider = new TestConfigurationProvider(_validOptions, null, null, (data, message) =>
        {
            capturedMessage = message;
            return Task.FromResult(true);
        });

        await provider.SaveAsync(new Dictionary<string, string?> { ["key"] = "value" }, "Custom save message");

        capturedMessage.Should().Be("Custom save message");
    }

    [Fact]
    public void ProcessContentBeforeLoad_WithEncryptionDisabled_ReturnsOriginalContent()
    {
        var encryptionService = new AesEncryptionService("test-key");
        var options = new TestGitRepositoryOptions
        {
            AccessToken = "token",
            Owner = "owner",
            Repo = "repo",
            EnableEncryption = false,
            EncryptionKey = "test-key"
        };
        var provider = new TestConfigurationProvider(options, encryptionService);
        var content = "test content";

        var result = provider.ExposeProcessContentBeforeLoad(content);

        result.Should().Be(content);
    }

    [Fact]
    public void ProcessContentBeforeSave_WithEncryptionDisabled_ReturnsOriginalContent()
    {
        var encryptionService = new AesEncryptionService("test-key");
        var options = new TestGitRepositoryOptions
        {
            AccessToken = "token",
            Owner = "owner",
            Repo = "repo",
            EnableEncryption = false,
            EncryptionKey = "test-key"
        };
        var provider = new TestConfigurationProvider(options, encryptionService);
        var content = "test content";

        var result = provider.ExposeProcessContentBeforeSave(content);

        result.Should().Be(content);
    }

    [Fact]
    public void ProcessContentBeforeLoad_WithEncryptionEnabledButNoService_ReturnsOriginalContent()
    {
        var options = new TestGitRepositoryOptions
        {
            AccessToken = "token",
            Owner = "owner",
            Repo = "repo",
            EnableEncryption = true,
            EncryptionKey = "test-key"
        };
        var provider = new TestConfigurationProvider(options, null);
        var content = "test content";

        var result = provider.ExposeProcessContentBeforeLoad(content);

        result.Should().Be(content);
    }

    [Fact]
    public void ProcessContentBeforeSave_WithEncryptionEnabledButNoService_ReturnsOriginalContent()
    {
        var options = new TestGitRepositoryOptions
        {
            AccessToken = "token",
            Owner = "owner",
            Repo = "repo",
            EnableEncryption = true,
            EncryptionKey = "test-key"
        };
        var provider = new TestConfigurationProvider(options, null);
        var content = "test content";

        var result = provider.ExposeProcessContentBeforeSave(content);

        result.Should().Be(content);
    }

    [Fact]
    public void SetData_WithEmptyDictionary_SetsEmptyData()
    {
        var provider = new TestConfigurationProvider(_validOptions, null);

        provider.ExposeSetData(new Dictionary<string, string?>(), false);

        provider.TryGet("any-key", out _).Should().BeFalse();
    }

    [Fact]
    public void SetData_WithNullValues_HandlesNullValues()
    {
        var provider = new TestConfigurationProvider(_validOptions, null);

        provider.ExposeSetData(new Dictionary<string, string?> { ["key"] = null }, false);

        provider.TryGet("key", out var value).Should().BeTrue();
        value.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_WithExistingKey_UpdatesValue()
    {
        var savedData = (Dictionary<string, string?>?)null;
        var provider = new TestConfigurationProvider(_validOptions, null, null, (data, message) =>
        {
            savedData = data;
            return Task.FromResult(true);
        });
        provider.ExposeSetData(new Dictionary<string, string?> { ["key"] = "oldValue" }, false);

        var result = await provider.SetAsync("key", "newValue");

        result.Should().BeTrue();
        savedData!["key"].Should().Be("newValue");
    }

    [Fact]
    public async Task RemoveAsync_WithNonExistentKey_ReturnsTrue()
    {
        var savedData = (Dictionary<string, string?>?)null;
        var provider = new TestConfigurationProvider(_validOptions, null, null, (data, message) =>
        {
            savedData = data;
            return Task.FromResult(true);
        });
        provider.ExposeSetData(new Dictionary<string, string?> { ["key"] = "value" }, false);

        var result = await provider.RemoveAsync("nonExistentKey");

        result.Should().BeTrue();
        savedData.Should().ContainKey("key");
    }

    [Fact]
    public void Load_WithExceptionInLoadAsync_PropagatesException()
    {
        var provider = new TestConfigurationProvider(_validOptions, null, () =>
        {
            throw new InvalidOperationException("Load failed");
        });

        var act = () => provider.Load();
        act.Should().Throw<InvalidOperationException>().WithMessage("Load failed");
    }

    [Fact]
    public async Task ReloadAsync_WithExceptionInLoadAsync_PropagatesException()
    {
        var provider = new TestConfigurationProvider(_validOptions, null, () =>
        {
            throw new InvalidOperationException("Reload failed");
        });

        var act = () => provider.ReloadAsync();
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Reload failed");
    }

    [Fact]
    public async Task OnWebhookReceivedAsync_WithExceptionInLoadAsync_PropagatesException()
    {
        var provider = new TestConfigurationProvider(_validOptions, null, () =>
        {
            throw new InvalidOperationException("Webhook load failed");
        });

        var act = () => provider.OnWebhookReceivedAsync();
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Webhook load failed");
    }
}
