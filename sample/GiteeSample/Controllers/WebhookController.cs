using Cyoukon.Extensions.Configuration.Gitee;
using Microsoft.AspNetCore.Mvc;

namespace GiteeSample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly IConfigurationRoot _configurationRoot;
    private readonly IConfiguration _configuration;

    public WebhookController(IConfiguration configuration)
    {
        _configuration = configuration;
        _configurationRoot = (IConfigurationRoot)configuration;
    }

    [HttpPost]
    public async Task<IActionResult> HandleWebhook()
    {
        var provider = _configurationRoot.GetGiteeConfigurationProvider();
        if (provider == null)
        {
            return BadRequest(new { Message = "Gitee configuration provider not found" });
        }

        var options = new GiteeConfigurationOptions
        {
            AccessToken = _configuration["Gitee:AccessToken"] ?? string.Empty,
            Owner = _configuration["Gitee:Owner"] ?? string.Empty,
            Repo = _configuration["Gitee:Repo"] ?? string.Empty,
            Branch = _configuration["Gitee:Branch"] ?? "main",
            ConfigPath = _configuration["Gitee:ConfigPath"] ?? "config",
            ConfigFileName = _configuration["Gitee:ConfigFileName"] ?? "appsettings.json",
            WebhookSecret = _configuration["Gitee:WebhookSecret"]
        };

        var handler = new GiteeWebhookHandler(provider, options);

        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();

        var timestamp = Request.Headers["X-Gitee-Timestamp"].FirstOrDefault();
        var signature = Request.Headers["X-Gitee-Signature"].FirstOrDefault();
        var eventType = Request.Headers["X-Gitee-Event"].FirstOrDefault();

        var result = await handler.HandleWebhookAsync(payload, timestamp, signature, eventType).ConfigureAwait(false);

        if (!result.Success)
        {
            return BadRequest(new { Message = result.Message });
        }

        return Ok(new
        {
            Message = result.Message,
            ConfigurationChanged = result.ConfigurationChanged,
            ChangedFiles = result.ChangedFiles
        });
    }
}
