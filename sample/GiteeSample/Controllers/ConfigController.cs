using Cyoukon.Extensions.Configuration.Gitee;
using Microsoft.AspNetCore.Mvc;

namespace GiteeSample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IConfigurationRoot _configurationRoot;

    public ConfigController(IConfiguration configuration)
    {
        _configuration = configuration;
        _configurationRoot = (IConfigurationRoot)configuration;
    }

    [HttpGet]
    public IActionResult GetAllConfig()
    {
        var configs = new Dictionary<string, string?>();
        foreach (var item in _configuration.AsEnumerable())
        {
            if (!item.Key.StartsWith("Gitee", StringComparison.OrdinalIgnoreCase) &&
                !item.Key.StartsWith("GitConfiguration", StringComparison.OrdinalIgnoreCase))
            {
                configs[item.Key] = item.Value;
            }
        }
        return Ok(configs);
    }

    [HttpGet("{key}")]
    public IActionResult GetConfig(string key)
    {
        var value = _configuration[key];
        if (value == null)
        {
            return NotFound(new { Message = $"Configuration key '{key}' not found" });
        }
        return Ok(new { Key = key, Value = value });
    }

    [HttpPost("reload")]
    public async Task<IActionResult> ReloadConfiguration()
    {
        var provider = _configurationRoot.GetGiteeConfigurationProvider();
        if (provider == null)
        {
            return BadRequest(new { Message = "Gitee configuration provider not found" });
        }

        await provider.ReloadAsync().ConfigureAwait(false);
        return Ok(new { Message = "Configuration reloaded successfully" });
    }
}
