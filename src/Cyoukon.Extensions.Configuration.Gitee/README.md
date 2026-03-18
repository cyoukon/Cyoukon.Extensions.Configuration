# Cyoukon.Extensions.Configuration.Gitee

基于 Gitee 仓库的自定义配置提供程序，支持通过 Gitee API 读写配置、加密存储以及 Webhook 自动更新。

## 功能特性

- 📁 **Gitee 仓库配置源** - 从 Gitee 指定仓库的指定目录读取配置文件
- 🔄 **读写支持** - 通过 Gitee API 实现配置的读取和写入
- 🔐 **加密存储** - 支持 AES-256 加密配置内容后存储到 Gitee
- 🔔 **Webhook 支持** - 接收 Gitee Webhook 通知，自动重新加载配置
- 🔒 **签名验证** - 支持 Webhook 密钥签名验证，确保请求安全可信
- ⏱️ **自动重载** - 支持定时轮询配置变更

## 安装

通过 NuGet 安装：

```bash
dotnet add package Cyoukon.Extensions.Configuration.Gitee
```

## 快速开始

### 基本使用

```csharp
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationManager();
configuration.AddGiteeConfiguration(options =>
{
    options.AccessToken = "your-gitee-access-token";
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
  "GiteeConfiguration": {
    "AccessToken": "your-gitee-access-token",
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

// 自动从 "GiteeConfiguration" 节点读取配置
configuration.AddGiteeConfiguration();

// 或指定其他节点名称
configuration.AddGiteeConfiguration("MyGiteeConfig");
```

### 在 ASP.NET Core 中使用

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddGiteeConfiguration(options =>
{
    options.AccessToken = builder.Configuration["Gitee:AccessToken"];
    options.Owner = builder.Configuration["Gitee:Owner"];
    options.Repo = builder.Configuration["Gitee:Repo"];
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
  "GiteeConfiguration": {
    "AccessToken": "your-gitee-access-token",
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

// 自动从配置文件读取 Gitee 配置
builder.Configuration.AddGiteeConfiguration();

var app = builder.Build();
app.Run();
```

### 启用加密存储

```csharp
configuration.AddGiteeConfiguration(options =>
{
    options.AccessToken = "your-gitee-access-token";
    options.Owner = "your-username";
    options.Repo = "your-repo";
    options.EnableEncryption = true;
    options.EncryptionKey = "your-secret-encryption-key";
});
```

### 写入配置

```csharp
using Cyoukon.Extensions.Configuration.Gitee;

// 获取 Provider 实例
var provider = ((IConfigurationRoot)configuration).GetGiteeConfigurationProvider();

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

配置 Gitee Webhook 后，在应用中处理推送事件：

```csharp
using Cyoukon.Extensions.Configuration.Gitee;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly GiteeConfigurationOptions _options;

    public WebhookController(IConfiguration configuration, GiteeConfigurationOptions options)
    {
        _configuration = configuration;
        _options = options;
    }

    [HttpPost("gitee")]
    public IActionResult HandleGiteeWebhook([FromBody] string payload)
    {
        var handler = ((IConfigurationRoot)_configuration).CreateWebhookHandler(_options);
        
        if (handler == null)
        {
            return BadRequest("Gitee configuration provider not found");
        }

        var result = handler.HandleWebhook(payload);
        
        if (result.Success && result.ConfigurationChanged)
        {
            // 配置已重新加载
            return Ok(new { message = result.Message, changedFiles = result.ChangedFiles });
        }

        return Ok(new { message = result.Message });
    }
}
```

### Webhook 签名验证（推荐）

启用签名验证可以确保 Webhook 请求来自 Gitee，防止恶意请求：

```csharp
using Cyoukon.Extensions.Configuration.Gitee;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly GiteeConfigurationOptions _options;

    public WebhookController(IConfiguration configuration, GiteeConfigurationOptions options)
    {
        _configuration = configuration;
        _options = options;
    }

    [HttpPost("gitee")]
    public IActionResult HandleGiteeWebhook(
        [FromBody] string payload,
        [FromHeader(Name = "X-Gitee-Timestamp")] string? timestamp,
        [FromHeader(Name = "X-Gitee-Signature")] string? signature)
    {
        var handler = ((IConfigurationRoot)_configuration).CreateWebhookHandler(_options);
        
        if (handler == null)
        {
            return BadRequest("Gitee configuration provider not found");
        }

        // 使用签名验证
        var result = handler.HandleWebhook(payload, timestamp, signature);
        
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
builder.Configuration.AddGiteeConfiguration(options =>
{
    options.AccessToken = builder.Configuration["Gitee:AccessToken"];
    options.Owner = builder.Configuration["Gitee:Owner"];
    options.Repo = builder.Configuration["Gitee:Repo"];
    options.WebhookSecret = builder.Configuration["Gitee:WebhookSecret"]; // Webhook 密钥
});
```

## 配置选项

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `AccessToken` | `string` | 必填 | Gitee API 访问令牌 |
| `Owner` | `string` | 必填 | 仓库所有者 |
| `Repo` | `string` | 必填 | 仓库名称 |
| `Branch` | `string` | `"master"` | 分支名称 |
| `ConfigPath` | `string` | `"config"` | 配置文件所在目录 |
| `ConfigFileName` | `string` | `"appsettings.json"` | 配置文件名 |
| `EnableEncryption` | `bool` | `false` | 是否启用加密 |
| `EncryptionKey` | `string?` | `null` | 加密密钥（启用加密时必填） |
| `EnableReload` | `bool` | `false` | 是否启用自动重载 |
| `ReloadInterval` | `TimeSpan` | `5分钟` | 自动重载间隔 |
| `ApiBaseUrl` | `string` | `"https://gitee.com/api/v5"` | Gitee API 地址 |
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

## Gitee Webhook 配置

1. 进入 Gitee 仓库设置页面
2. 找到 **WebHooks** 设置
3. 添加 Webhook：
   - URL: `https://your-domain.com/api/webhook/gitee`
   - 密码: 设置一个安全的密钥（用于签名验证）
   - 触发事件: 选择 **Push**
4. 保存设置

当配置文件被修改并推送到仓库时，应用会自动重新加载配置。

### 签名验证原理

Gitee Webhook 签名验证使用 HMAC-SHA256 算法：

1. Gitee 发送请求时，会在 HTTP Header 中包含：
   - `X-Gitee-Timestamp`: 时间戳
   - `X-Gitee-Signature`: 签名值

2. 签名算法：
   ```
   string_to_sign = "{timestamp}\n{secret}"
   signature = Base64(HMAC-SHA256(secret, string_to_sign))
   ```

3. 服务端使用相同的密钥计算签名，并与请求中的签名比对

## 获取 Gitee Access Token

1. 登录 Gitee
2. 进入 **设置** → **私人令牌**
3. 点击 **生成新令牌**
4. 选择所需权限：
   - `projects` - 读取仓库信息
   - `repo` - 读写仓库内容
5. 复制生成的令牌

## 注意事项

- 确保 Gitee Access Token 具有足够的权限
- 加密密钥请妥善保管，丢失后将无法解密配置
- 建议在生产环境中使用环境变量或密钥管理服务存储敏感信息
- 强烈建议启用 Webhook 签名验证以确保安全性

## License

MIT License
