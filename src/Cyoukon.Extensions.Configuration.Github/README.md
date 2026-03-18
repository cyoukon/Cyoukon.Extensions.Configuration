# Cyoukon.Extensions.Configuration.Github

基于 GitHub 仓库的自定义配置提供程序，支持通过 GitHub API 读写配置、加密存储以及 Webhook 自动更新。

## 功能特性

- 📁 **GitHub 仓库配置源** - 从 GitHub 指定仓库的指定目录读取配置文件
- 🔄 **读写支持** - 通过 GitHub API 实现配置的读取和写入
- 🔐 **加密存储** - 支持 AES-256 加密配置内容后存储到 GitHub
- 🔔 **Webhook 支持** - 接收 GitHub Webhook 通知，自动重新加载配置
- 🔒 **签名验证** - 支持 Webhook 密钥签名验证，确保请求安全可信
- ⏱️ **自动重载** - 支持定时轮询配置变更

## 安装

通过 NuGet 安装：

```bash
dotnet add package Cyoukon.Extensions.Configuration.Github
```

## 快速开始

### 基本使用

```csharp
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationManager();
configuration.AddGithubConfiguration(options =>
{
    options.AccessToken = "your-github-personal-access-token";
    options.Owner = "your-username";
    options.Repo = "your-repo";
    options.Branch = "main";
    options.ConfigPath = "config";
    options.ConfigFileName = "appsettings.json";
});

// 读取配置
var value = configuration["MySetting"];
```

### 从配置文件自动初始化（推荐）

如果配置信息已存在于配置文件中，可以直接从指定节点读取：

**appsettings.json:**
```json
{
  "GithubConfiguration": {
    "AccessToken": "your-github-personal-access-token",
    "Owner": "your-username",
    "Repo": "your-repo",
    "Branch": "main",
    "ConfigPath": "config",
    "ConfigFileName": "appsettings.json",
    "EnableReload": true,
    "ReloadInterval": "00:05:00"
  }
}
```

**代码:**
```csharp
var configuration = new ConfigurationManager();
configuration.AddJsonFile("appsettings.json");

// 自动从 "GithubConfiguration" 节点读取配置
configuration.AddGithubConfiguration();

// 或指定其他节点名称
configuration.AddGithubConfiguration("MyGithubConfig");
```

### 在 ASP.NET Core 中使用

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddGithubConfiguration(options =>
{
    options.AccessToken = builder.Configuration["Github:AccessToken"];
    options.Owner = builder.Configuration["Github:Owner"];
    options.Repo = builder.Configuration["Github:Repo"];
    options.Branch = "main";
    options.ConfigPath = "config";
    options.ConfigFileName = "appsettings.json";
    options.EnableReload = true;
    options.ReloadInterval = TimeSpan.FromMinutes(5);
});

var app = builder.Build();
app.Run();
```

或使用配置节点自动绑定：

**appsettings.json:**
```json
{
  "GithubConfiguration": {
    "AccessToken": "your-github-personal-access-token",
    "Owner": "your-username",
    "Repo": "your-repo",
    "Branch": "main",
    "ConfigPath": "config",
    "ConfigFileName": "appsettings.json",
    "EnableReload": true,
    "ReloadInterval": "00:05:00",
    "WebhookSecret": "your-webhook-secret"
  }
}
```

**Program.cs:**
```csharp
var builder = WebApplication.CreateBuilder(args);

// 自动从配置文件读取 GitHub 配置
builder.Configuration.AddGithubConfiguration();

var app = builder.Build();
app.Run();
```

### 启用加密存储

```csharp
configuration.AddGithubConfiguration(options =>
{
    options.AccessToken = "your-github-personal-access-token";
    options.Owner = "your-username";
    options.Repo = "your-repo";
    options.EnableEncryption = true;
    options.EncryptionKey = "your-secret-encryption-key";
});
```

### 写入配置

```csharp
using Cyoukon.Extensions.Configuration.Github;

// 获取 Provider 实例
var provider = ((IConfigurationRoot)configuration).GetGithubConfigurationProvider();

// 设置单个配置项
await provider.SetAsync("MySetting", "NewValue", "Update MySetting");

// 批量保存配置
var newConfig = new Dictionary<string, string?>
{
    ["App:Name"] = "MyApp",
    ["App:Version"] = "1.0.0",
    ["Database:ConnectionString"] = "Server=localhost;Database=mydb"
};
await provider.SaveAsync(newConfig, "Update application configuration");

// 删除配置项
await provider.RemoveAsync("OldSetting", "Remove deprecated setting");
```

### Webhook 集成

配置 GitHub Webhook 后，在应用中处理推送事件：

```csharp
using Cyoukon.Extensions.Configuration.Github;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly GithubConfigurationOptions _options;

    public WebhookController(IConfiguration configuration, GithubConfigurationOptions options)
    {
        _configuration = configuration;
        _options = options;
    }

    [HttpPost("github")]
    public IActionResult HandleGithubWebhook(
        [FromBody] string payload,
        [FromHeader(Name = "X-Hub-Signature-256")] string? signature)
    {
        var handler = ((IConfigurationRoot)_configuration).CreateWebhookHandler(_options);
        
        if (handler == null)
        {
            return BadRequest("GitHub configuration provider not found");
        }

        // 使用签名验证
        var result = handler.HandleWebhook(payload, signature);
        
        if (!result.Success)
        {
            return Unauthorized(new { message = result.Message });
        }

        if (result.ConfigurationChanged)
        {
            return Ok(new { message = result.Message, changedFiles = result.ChangedFiles });
        }

        return Ok(new { message = result.Message });
    }
}
```

配置选项中设置 Webhook 密钥：

```csharp
builder.Configuration.AddGithubConfiguration(options =>
{
    options.AccessToken = builder.Configuration["Github:AccessToken"];
    options.Owner = builder.Configuration["Github:Owner"];
    options.Repo = builder.Configuration["Github:Repo"];
    options.WebhookSecret = builder.Configuration["Github:WebhookSecret"]; // Webhook 密钥
});
```

## 配置选项

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `AccessToken` | `string` | 必填 | GitHub Personal Access Token |
| `Owner` | `string` | 必填 | 仓库所有者（用户名或组织名） |
| `Repo` | `string` | 必填 | 仓库名称 |
| `Branch` | `string` | `"main"` | 分支名称 |
| `ConfigPath` | `string` | `"config"` | 配置文件所在目录 |
| `ConfigFileName` | `string` | `"appsettings.json"` | 配置文件名 |
| `EnableEncryption` | `bool` | `false` | 是否启用加密 |
| `EncryptionKey` | `string?` | `null` | 加密密钥（启用加密时必填） |
| `EnableReload` | `bool` | `false` | 是否启用自动重载 |
| `ReloadInterval` | `TimeSpan` | `5分钟` | 自动重载间隔 |
| `ApiBaseUrl` | `string` | `"https://api.github.com"` | GitHub API 地址 |
| `ApiVersion` | `string` | `"2022-11-28"` | GitHub API 版本 |
| `WebhookSecret` | `string?` | `null` | Webhook 密钥（用于签名验证） |

## 配置文件格式

支持 JSON 格式的配置文件，例如：

```json
{
  "App": {
    "Name": "MyApplication",
    "Version": "1.0.0"
  },
  "Database": {
    "ConnectionString": "Server=localhost;Database=mydb",
    "Timeout": 30
  },
  "Features": {
    "EnableCache": true,
    "EnableLogging": true
  }
}
```

读取时将转换为扁平化的键值对：

```
App:Name = "MyApplication"
App:Version = "1.0.0"
Database:ConnectionString = "Server=localhost;Database=mydb"
Database:Timeout = "30"
Features:EnableCache = "true"
Features:EnableLogging = "true"
```

## GitHub Webhook 配置

1. 进入 GitHub 仓库设置页面
2. 找到 **Settings** → **Webhooks**
3. 点击 **Add webhook**：
   - Payload URL: `https://your-domain.com/api/webhook/github`
   - Content type: `application/json`
   - Secret: 设置一个安全的密钥（用于签名验证）
   - 触发事件: 选择 **Just the push event**
4. 点击 **Add webhook** 保存设置

当配置文件被修改并推送到仓库时，应用会自动重新加载配置。

### 签名验证原理

GitHub Webhook 签名验证使用 HMAC-SHA256 算法：

1. GitHub 发送请求时，会在 HTTP Header 中包含：
   - `X-Hub-Signature-256`: 签名值（格式：`sha256=<hex>`）

2. 签名算法：
   ```
   signature = "sha256=" + Hex(HMAC-SHA256(secret, payload))
   ```

3. 服务端使用相同的密钥计算签名，并与请求中的签名比对

## 获取 GitHub Personal Access Token

1. 登录 GitHub
2. 点击右上角头像 → **Settings**
3. 左侧菜单最下方 → **Developer settings**
4. 点击 **Personal access tokens** → **Tokens (classic)**
5. 点击 **Generate new token (classic)**
6. 设置 Token 名称和过期时间
7. 选择所需权限：
   - `repo` - 完整的仓库访问权限（推荐）
   - 或仅选择 `repo:status`、`repo_deployment`、`public_repo`、`repo:invite`
8. 点击 **Generate token**
9. **立即复制令牌**（离开页面后无法再次查看）

### Fine-grained Token 权限

如果使用 Fine-grained Personal Access Token，需要设置以下权限：

**Repository permissions:**
- **Contents**: Read and Write
- **Metadata**: Read

## 注意事项

- 确保 GitHub Personal Access Token 具有足够的权限
- 加密密钥请妥善保管，丢失后将无法解密配置
- 建议在生产环境中使用环境变量或密钥管理服务存储敏感信息
- 强烈建议启用 Webhook 签名验证以确保安全性
- GitHub API 有速率限制，建议合理设置 `ReloadInterval`

## 相关项目

- [Cyoukon.Extensions.Configuration.Gitee](../Cyoukon.Extensions.Configuration.Gitee) - Gitee 配置提供程序
- [Cyoukon.Extensions.Configuration.Abstractions](../Cyoukon.Extensions.Configuration.Abstractions) - 共享抽象库

## License

MIT License
