# Cyoukon.Extensions.Configuration.Abstractions

共享抽象层，为 Git 仓库配置提供程序（GitHub、Gitee）提供基础类和接口。

## 功能特性

- 📦 **GitConfigurationProviderBase** - Git 配置提供程序基类，封装了配置加载、保存、自动重载等核心逻辑
- 🔐 **IEncryptionService** - 加密服务接口
- 🔒 **AesEncryptionService** - AES-256 加密服务实现
- ⚙️ **GitRepositoryOptions** - Git 仓库配置选项基类
- 🔔 **WebhookResult** - Webhook 处理结果模型
- 🛡️ **WebhookSignatureVerifier** - Webhook 签名验证工具

## 安装

```bash
dotnet add package Cyoukon.Extensions.Configuration.Abstractions
```

## 核心类

### GitConfigurationProviderBase\<TOptions\>

Git 配置提供程序的抽象基类：

```csharp
public abstract class GitConfigurationProviderBase<TOptions> : ConfigurationProvider, IDisposable
    where TOptions : GitRepositoryOptions
{
    // 加载配置
    protected abstract Task LoadAsync();
    
    // 保存配置
    public abstract Task<bool> SaveAsync(Dictionary<string, string?> data, string commitMessage);
    
    // 设置单个配置项
    public async Task<bool> SetAsync(string key, string? value, string commitMessage);
    
    // 删除配置项
    public async Task<bool> RemoveAsync(string key, string commitMessage);
    
    // 重新加载
    public void Reload();
    
    // 处理 Webhook
    public void OnWebhookReceived();
}
```

### GitRepositoryOptions

Git 仓库配置选项基类：

```csharp
public abstract class GitRepositoryOptions
{
    public string AccessToken { get; set; }
    public string Owner { get; set; }
    public string Repo { get; set; }
    public string Branch { get; set; } = "main";
    public string ConfigPath { get; set; } = "config";
    public string ConfigFileName { get; set; } = "appsettings.json";
    public bool EnableEncryption { get; set; }
    public string? EncryptionKey { get; set; }
    public bool EnableReload { get; set; }
    public TimeSpan ReloadInterval { get; set; } = TimeSpan.FromMinutes(5);
    public string? WebhookSecret { get; set; }
    
    public virtual void Validate();
}
```

### IEncryptionService

加密服务接口：

```csharp
public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
```

### AesEncryptionService

AES-256 加密服务实现：

```csharp
var encryptionService = new AesEncryptionService("your-secret-key");
var encrypted = encryptionService.Encrypt("plain text");
var decrypted = encryptionService.Decrypt(encrypted);
```

### WebhookResult

Webhook 处理结果：

```csharp
public class WebhookResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public bool ConfigurationChanged { get; set; }
    public string[]? ChangedFiles { get; set; }
}
```

## 实现自定义配置提供程序

继承 `GitConfigurationProviderBase` 实现自定义 Git 平台配置提供程序：

```csharp
public class MyGitConfigurationProvider : GitConfigurationProviderBase<MyGitOptions>
{
    private readonly IMyGitApiClient _apiClient;
    
    public MyGitConfigurationProvider(MyGitOptions options, IMyGitApiClient apiClient)
        : base(options, null)
    {
        _apiClient = apiClient;
    }
    
    protected override async Task LoadAsync()
    {
        var content = await _apiClient.GetFileContentAsync(
            Options.Owner, 
            Options.Repo, 
            Options.GetFilePath(), 
            Options.Branch);
        
        content = ProcessContentBeforeLoad(content);
        var data = ConfigurationParser.ParseJson(content);
        SetData(data, true);
    }
    
    public override async Task<bool> SaveAsync(Dictionary<string, string?> data, string commitMessage)
    {
        var json = ConfigurationParser.ToJson(data);
        json = ProcessContentBeforeSave(json);
        
        return await _apiClient.UpdateFileAsync(
            Options.Owner,
            Options.Repo,
            Options.GetFilePath(),
            json,
            commitMessage,
            Options.Branch);
    }
}
```

## 相关项目

- [Cyoukon.Extensions.Configuration.Github](../Cyoukon.Extensions.Configuration.Github) - GitHub 配置提供程序
- [Cyoukon.Extensions.Configuration.Gitee](../Cyoukon.Extensions.Configuration.Gitee) - Gitee 配置提供程序

## License

MIT License
