using Cyoukon.Extensions.Configuration.Abstractions.Services;
using FluentAssertions;
using Xunit;

namespace Cyoukon.Extensions.Configuration.Abstractions.Tests.Services;

public class AesEncryptionServiceTests
{
    [Fact]
    public void Constructor_WithNullKey_ThrowsArgumentNullException()
    {
        var act = () => new AesEncryptionService(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithEmptyKey_ThrowsArgumentNullException()
    {
        var act = () => new AesEncryptionService(string.Empty);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithValidKey_CreatesInstance()
    {
        var service = new AesEncryptionService("test-key");
        service.Should().NotBeNull();
    }

    [Fact]
    public void Encrypt_WithNullText_ReturnsNull()
    {
        var service = new AesEncryptionService("test-key");
        var result = service.Encrypt(null!);
        result.Should().BeNull();
    }

    [Fact]
    public void Encrypt_WithEmptyText_ReturnsEmpty()
    {
        var service = new AesEncryptionService("test-key");
        var result = service.Encrypt(string.Empty);
        result.Should().BeEmpty();
    }

    [Fact]
    public void Encrypt_WithValidText_ReturnsEncryptedString()
    {
        var service = new AesEncryptionService("test-key");
        var plainText = "Hello, World!";
        var result = service.Encrypt(plainText);
        result.Should().NotBeNullOrEmpty();
        result.Should().NotBe(plainText);
    }

    [Fact]
    public void Decrypt_WithNullText_ReturnsNull()
    {
        var service = new AesEncryptionService("test-key");
        var result = service.Decrypt(null!);
        result.Should().BeNull();
    }

    [Fact]
    public void Decrypt_WithEmptyText_ReturnsEmpty()
    {
        var service = new AesEncryptionService("test-key");
        var result = service.Decrypt(string.Empty);
        result.Should().BeEmpty();
    }

    [Fact]
    public void Decrypt_WithValidEncryptedText_ReturnsOriginalText()
    {
        var service = new AesEncryptionService("test-key");
        var plainText = "Hello, World!";
        var encrypted = service.Encrypt(plainText);
        var decrypted = service.Decrypt(encrypted);
        decrypted.Should().Be(plainText);
    }

    [Fact]
    public void Decrypt_WithInvalidBase64_ThrowsFormatException()
    {
        var service = new AesEncryptionService("test-key");
        var act = () => service.Decrypt("not-valid-base64!!!");
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void EncryptDecrypt_WithDifferentKeys_ProducesDifferentResults()
    {
        var service1 = new AesEncryptionService("key1");
        var service2 = new AesEncryptionService("key2");
        var plainText = "Hello, World!";
        
        var encrypted1 = service1.Encrypt(plainText);
        var encrypted2 = service2.Encrypt(plainText);
        
        encrypted1.Should().NotBe(encrypted2);
    }

    [Fact]
    public void EncryptDecrypt_WithSameKey_ProducesConsistentResults()
    {
        var service = new AesEncryptionService("test-key");
        var plainText = "Hello, World!";
        
        var encrypted1 = service.Encrypt(plainText);
        var encrypted2 = service.Encrypt(plainText);
        
        encrypted1.Should().Be(encrypted2);
    }

    [Theory]
    [InlineData("simple text")]
    [InlineData("文本测试")]
    [InlineData("emoji 🎉")]
    [InlineData("special chars !@#$%^&*()")]
    public void EncryptDecrypt_WithVariousInputs_RoundTripsCorrectly(string input)
    {
        var service = new AesEncryptionService("test-key");
        var encrypted = service.Encrypt(input);
        var decrypted = service.Decrypt(encrypted);
        decrypted.Should().Be(input);
    }

    [Fact]
    public void Encrypt_WithLongText_HandlesCorrectly()
    {
        var service = new AesEncryptionService("test-key");
        var plainText = new string('a', 10000);
        var encrypted = service.Encrypt(plainText);
        var decrypted = service.Decrypt(encrypted);
        decrypted.Should().Be(plainText);
    }
}
