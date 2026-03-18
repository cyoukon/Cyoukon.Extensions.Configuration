# Cyoukon.Extensions.Configuration

基于 Git 仓库（GitHub / Gitee）的分布式配置管理库，支持从远程仓库读取配置文件、热更新、加密存储和 Webhook 自动刷新。

## 功能特性

- 📁 **Git 仓库配置源** - 从 GitHub 或 Gitee 仓库读取配置文件
- 🔄 **读写支持** - 通过 GitHub/Gitee API 实现配置的读取和写入
- 🔐 **加密存储** - 支持 AES-256 加密配置内容后存储到远程仓库
- 🔔 **Webhook 支持** - 接收 Webhook 通知，自动重新加载配置
- 🔒 **签名验证** - 支持 Webhook 签名验证，确保请求安全可信
- ⏱️ **自动重载** - 支持定时轮询配置变更

## 支持的平台

| 包 | NuGet | 说明 |
|---|-------|------|
| `Cyoukon.Extensions.Configuration.Github` | [![NuGet](https://img.shields.io/nuget/v/Cyoukon.Extensions.Configuration.Github.svg)](https://www.nuget.org/packages/Cyoukon.Extensions.Configuration.Github) | GitHub 配置提供程序 |
| `Cyoukon.Extensions.Configuration.Gitee` | [![NuGet](https://img.shields.io/nuget/v/Cyoukon.Extensions.Configuration.Gitee.svg)](https://www.nuget.org/packages/Cyoukon.Extensions.Configuration.Gitee) | Gitee 配置提供程序 |
| `Cyoukon.Extensions.Configuration.Abstractions` | - | 共享抽象层 |

## 快速开始

### 安装

```bash
# GitHub 版本
dotnet add package Cyoukon.Extensions.Configuration.Github

# Gitee 版本
dotnet add package Cyoukon.Extensions.Configuration.Gitee
```

### 使用示例

#### 基本使用

```csharp
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationManager();

// 添加 GitHub 配置源
configuration.AddGithubConfiguration(options =>
{
    options.AccessToken = "your-github-token";
    options.Owner = "your-username";
    options.Repo = "your-repo";
    options.Branch = "main";
    options.ConfigPath = "config";
    options.ConfigFileName = "appsettings.json";
});

// 读取配置
var value = configuration["MySetting"];
```

#### 在 ASP.NET Core 中使用

**appsettings.json:**
```json
{
  "GithubConfiguration": {
    "AccessToken": "your-github-token",
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

**Program.cs:**
```csharp
var builder = WebApplication.CreateBuilder(args);

// 自动从配置文件读取配置
builder.Configuration.AddGithubConfiguration();

var app = builder.Build();
app.Run();
```

#### 启用加密存储

```csharp
configuration.AddGithubConfiguration(options =>
{
    options.AccessToken = "your-github-token";
    options.Owner = "your-username";
    options.Repo = "your-repo";
    options.EnableEncryption = true;
    options.EncryptionKey = "your-secret-key";
});
```

#### Webhook 自动刷新

配置 Webhook 后，应用会在配置变更时自动重载：

```csharp
// GitHub
configuration.AddGithubConfiguration(options =>
{
    options.AccessToken = "your-github-token";
    options.Owner = "your-username";
    options.Repo = "your-repo";
    options.WebhookSecret = "your-webhook-secret"; // 用于签名验证
});
```

#### 写入配置

```csharp
using Cyoukon.Extensions.Configuration.Github;

// 获取 Provider 实例
var provider = ((IConfigurationRoot)configuration).GetGithubConfigurationProvider();

// 保存配置
await provider.SaveAsync(new Dictionary<string, string?>
{
    ["App:Name"] = "MyApp",
    ["App:Version"] = "1.0.0"
}, "Update configuration");
```

## 配置选项

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `AccessToken` | `string` | 必填 | Git 平台访问令牌 |
| `Owner` | `string` | 必填 | 仓库所有者 |
| `Repo` | `string` | 必填 | 仓库名称 |
| `Branch` | `string` | `"main"` | 分支名称 |
| `ConfigPath` | `string` | `"config"` | 配置文件目录 |
| `ConfigFileName` | `string` | `"appsettings.json"` | 配置文件名 |
| `EnableEncryption` | `bool` | `false` | 是否启用加密 |
| `EncryptionKey` | `string?` | `null` | 加密密钥 |
| `EnableReload` | `bool` | `false` | 是否启用自动重载 |
| `ReloadInterval` | `TimeSpan` | `5分钟` | 自动重载间隔 |
| `WebhookSecret` | `string?` | `null` | Webhook 密钥 |

## Webhook 配置

### GitHub

1. 进入仓库 **Settings** → **Webhooks**
2. 点击 **Add webhook**:
   - Payload URL: `https://your-domain.com/api/webhook/github`
   - Content type: `application/json`
   - Secret: 设置密钥
   - 触发事件: **Just the push event**
3. 保存

### Gitee

1. 进入仓库 **设置** → **WebHooks**
2. 添加 Webhook:
   - URL: `https://your-domain.com/api/webhook/gitee`
   - 密码: 设置密钥
   - 触发事件: **Push**
3. 保存

## 相关项目

- [Cyoukon.Extensions.Configuration.Github](./src/Cyoukon.Extensions.Configuration.Github) - GitHub 配置提供程序
- [Cyoukon.Extensions.Configuration.Gitee](./src/Cyoukon.Extensions.Configuration.Gitee) - Gitee 配置提供程序

## License

MIT License - see [LICENSE](./LICENSE)
