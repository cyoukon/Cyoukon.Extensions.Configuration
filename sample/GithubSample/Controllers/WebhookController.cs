using Cyoukon.Extensions.Configuration.Github;
using Microsoft.AspNetCore.Mvc;

namespace GithubSample.Controllers;

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
        var provider = _configurationRoot.GetGithubConfigurationProvider();
        if (provider == null)
        {
            return BadRequest(new { Message = "Github configuration provider not found" });
        }

        var options = new GithubConfigurationOptions
        {
            AccessToken = _configuration["Github:AccessToken"] ?? string.Empty,
            Owner = _configuration["Github:Owner"] ?? string.Empty,
            Repo = _configuration["Github:Repo"] ?? string.Empty,
            Branch = _configuration["Github:Branch"] ?? "main",
            ConfigPath = _configuration["Github:ConfigPath"] ?? "config",
            ConfigFileName = _configuration["Github:ConfigFileName"] ?? "appsettings.json",
            WebhookSecret = _configuration["Github:WebhookSecret"]
        };

        var handler = new GithubWebhookHandler(provider, options);

        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();

        var signature = Request.Headers["X-Hub-Signature-256"].FirstOrDefault();
        var eventType = Request.Headers["X-GitHub-Event"].FirstOrDefault();

        var result = await handler.HandleWebhookAsync(payload, signature).ConfigureAwait(false);

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
