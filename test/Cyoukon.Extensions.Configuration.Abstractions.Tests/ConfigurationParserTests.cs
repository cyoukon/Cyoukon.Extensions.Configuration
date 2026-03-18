using FluentAssertions;
using Xunit;

namespace Cyoukon.Extensions.Configuration.Abstractions.Tests;

public class ConfigurationParserTests
{
    [Fact]
    public void ParseJsonContent_WithValidJson_ReturnsParsedDictionary()
    {
        var json = @"{""key1"": ""value1"", ""key2"": ""value2""}";
        var result = ConfigurationParser.ParseJsonContent(json);
        
        result.Should().ContainKey("key1");
        result["key1"].Should().Be("value1");
        result.Should().ContainKey("key2");
        result["key2"].Should().Be("value2");
    }

    [Fact]
    public void ParseJsonContent_WithNestedObject_ReturnsFlattenedDictionary()
    {
        var json = @"{""parent"": {""child"": ""value""}}";
        var result = ConfigurationParser.ParseJsonContent(json);
        
        result.Should().ContainKey("parent:child");
        result["parent:child"].Should().Be("value");
    }

    [Fact]
    public void ParseJsonContent_WithArray_ReturnsIndexedDictionary()
    {
        var json = @"{""items"": [""a"", ""b"", ""c""]}";
        var result = ConfigurationParser.ParseJsonContent(json);
        
        result.Should().ContainKey("items:0");
        result["items:0"].Should().Be("a");
        result.Should().ContainKey("items:1");
        result["items:1"].Should().Be("b");
        result.Should().ContainKey("items:2");
        result["items:2"].Should().Be("c");
    }

    [Fact]
    public void ParseJsonContent_WithNumber_ReturnsNumberAsString()
    {
        var json = @"{""count"": 42}";
        var result = ConfigurationParser.ParseJsonContent(json);
        
        result.Should().ContainKey("count");
        result["count"].Should().Be("42");
    }

    [Fact]
    public void ParseJsonContent_WithBooleanTrue_ReturnsTrueString()
    {
        var json = @"{""enabled"": true}";
        var result = ConfigurationParser.ParseJsonContent(json);
        
        result.Should().ContainKey("enabled");
        result["enabled"].Should().Be("true");
    }

    [Fact]
    public void ParseJsonContent_WithBooleanFalse_ReturnsFalseString()
    {
        var json = @"{""enabled"": false}";
        var result = ConfigurationParser.ParseJsonContent(json);
        
        result.Should().ContainKey("enabled");
        result["enabled"].Should().Be("false");
    }

    [Fact]
    public void ParseJsonContent_WithNull_ReturnsNullValue()
    {
        var json = @"{""value"": null}";
        var result = ConfigurationParser.ParseJsonContent(json);
        
        result.Should().ContainKey("value");
        result["value"].Should().BeNull();
    }

    [Fact]
    public void ParseJsonContent_WithInvalidJson_ReturnsContentAsValue()
    {
        var invalidJson = "not valid json";
        var result = ConfigurationParser.ParseJsonContent(invalidJson);
        
        result.Should().ContainKey("Content");
        result["Content"].Should().Be(invalidJson);
    }

    [Fact]
    public void ParseJsonContent_WithComplexNestedStructure_ParsesCorrectly()
    {
        var json = @"{
            ""app"": {
                ""name"": ""TestApp"",
                ""version"": ""1.0"",
                ""settings"": {
                    ""debug"": true,
                    ""timeout"": 30
                }
            },
            ""servers"": [
                { ""host"": ""localhost"", ""port"": 8080 },
                { ""host"": ""remote"", ""port"": 9090 }
            ]
        }";
        
        var result = ConfigurationParser.ParseJsonContent(json);
        
        result["app:name"].Should().Be("TestApp");
        result["app:version"].Should().Be("1.0");
        result["app:settings:debug"].Should().Be("true");
        result["app:settings:timeout"].Should().Be("30");
        result["servers:0:host"].Should().Be("localhost");
        result["servers:0:port"].Should().Be("8080");
        result["servers:1:host"].Should().Be("remote");
        result["servers:1:port"].Should().Be("9090");
    }

    [Fact]
    public void SerializeToJson_WithSimpleDictionary_ReturnsValidJson()
    {
        var data = new Dictionary<string, string?>
        {
            ["key1"] = "value1",
            ["key2"] = "value2"
        };
        
        var result = ConfigurationParser.SerializeToJson(data);
        
        result.Should().Contain("\"key1\"");
        result.Should().Contain("\"value1\"");
        result.Should().Contain("\"key2\"");
        result.Should().Contain("\"value2\"");
    }

    [Fact]
    public void SerializeToJson_WithNestedKeys_ReturnsNestedJson()
    {
        var data = new Dictionary<string, string?>
        {
            ["parent:child"] = "value"
        };
        
        var result = ConfigurationParser.SerializeToJson(data);
        
        result.Should().Contain("\"parent\"");
        result.Should().Contain("\"child\"");
        result.Should().Contain("\"value\"");
    }

    [Fact]
    public void SerializeToJson_WithNullValue_IncludesNull()
    {
        var data = new Dictionary<string, string?>
        {
            ["key"] = null
        };
        
        var result = ConfigurationParser.SerializeToJson(data);
        
        result.Should().Contain("\"key\"");
        result.Should().Contain("null");
    }

    [Fact]
    public void DecodeBase64Content_WithValidBase64_ReturnsDecodedString()
    {
        var base64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("Hello, World!"));
        var result = ConfigurationParser.DecodeBase64Content(base64);
        result.Should().Be("Hello, World!");
    }

    [Fact]
    public void DecodeBase64Content_WithUnicode_ReturnsDecodedString()
    {
        var text = "你好世界";
        var base64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(text));
        var result = ConfigurationParser.DecodeBase64Content(base64);
        result.Should().Be(text);
    }

    [Fact]
    public void EncodeToBase64_WithString_ReturnsBase64String()
    {
        var text = "Hello, World!";
        var result = ConfigurationParser.EncodeToBase64(text);
        var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(result));
        decoded.Should().Be(text);
    }

    [Fact]
    public void EncodeDecode_RoundTrip_PreservesContent()
    {
        var original = "Test content with special chars: !@#$%^&*()";
        var encoded = ConfigurationParser.EncodeToBase64(original);
        var decoded = ConfigurationParser.DecodeBase64Content(encoded);
        decoded.Should().Be(original);
    }

    [Fact]
    public void ParseJsonContent_WithEmptyObject_ReturnsEmptyDictionary()
    {
        var json = "{}";
        var result = ConfigurationParser.ParseJsonContent(json);
        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseJsonContent_WithEmptyArray_ReturnsEmptyDictionary()
    {
        var json = @"{""items"": []}";
        var result = ConfigurationParser.ParseJsonContent(json);
        result.Should().NotContainKey("items:0");
    }

    [Fact]
    public void ParseJsonContent_IsCaseInsensitive()
    {
        var json = @"{""Key"": ""value""}";
        var result = ConfigurationParser.ParseJsonContent(json);
        result.Should().ContainKey("KEY");
        result.Should().ContainKey("key");
    }
}
