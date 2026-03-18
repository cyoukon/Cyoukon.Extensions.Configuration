using FluentAssertions;
using Xunit;

namespace Cyoukon.Extensions.Configuration.Abstractions.Tests;

public class WebhookResultTests
{
    [Fact]
    public void WebhookResult_DefaultValues_AreCorrect()
    {
        var result = new WebhookResult();
        
        result.Success.Should().BeFalse();
        result.Message.Should().BeNull();
        result.ConfigurationChanged.Should().BeFalse();
        result.ChangedFiles.Should().BeNull();
    }

    [Fact]
    public void WebhookResult_CanSetProperties()
    {
        var result = new WebhookResult
        {
            Success = true,
            Message = "Test message",
            ConfigurationChanged = true,
            ChangedFiles = new[] { "file1.json", "file2.json" }
        };
        
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Test message");
        result.ConfigurationChanged.Should().BeTrue();
        result.ChangedFiles.Should().ContainInOrder("file1.json", "file2.json");
    }
}
