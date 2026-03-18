using Cyoukon.Extensions.Configuration.Gitee;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configurationManager = new ConfigurationManager();
configurationManager.SetBasePath(Directory.GetCurrentDirectory());
configurationManager.AddJsonFile("appsettings.json", optional: false);
configurationManager.AddJsonFile($"appsettings.Development.json", optional: true);

configurationManager.AddGiteeConfiguration();

IConfiguration configuration = configurationManager;

var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);
services.AddOptions();
services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

var serviceProvider = services.BuildServiceProvider();

Console.WriteLine("=== Gitee Configuration Sample ===");
Console.WriteLine();

var appSettings = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AppSettings>>().Value;
Console.WriteLine($"Application Name: {appSettings.Name}");
Console.WriteLine($"Application Version: {appSettings.Version}");
Console.WriteLine($"Environment: {appSettings.Environment}");
Console.WriteLine();

Console.WriteLine("All Configuration Values:");
foreach (var item in configuration.AsEnumerable().OrderBy(x => x.Key))
{
    if (!item.Key.StartsWith("GiteeConfiguration", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine($"  {item.Key} = {item.Value}");
    }
}
Console.WriteLine();

var configurationRoot = (IConfigurationRoot)configuration;
var provider = configurationRoot.GetGiteeConfigurationProvider();
if (provider != null)
{
    Console.WriteLine("Provider found. You can:");
    Console.WriteLine("  - Call provider.Reload() to reload configuration");
    Console.WriteLine("  - Call provider.SetAsync(key, value) to update a value");
    Console.WriteLine("  - Call provider.RemoveAsync(key) to remove a value");

    await provider.SaveAsync(Enumerable.Range(0, 9)
        .ToDictionary(e => $"Test:Data{e}", e => (string?)DateTime.Now.Ticks.ToString()));

    await provider.SetAsync("Test1:Data1:Child1", DateTime.Now.ToString());
}

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();

public class AppSettings
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
}
