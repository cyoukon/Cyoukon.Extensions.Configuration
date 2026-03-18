using FluentAssertions;
using Xunit;

namespace Cyoukon.Extensions.Configuration.Abstractions.Tests;

public class WebhookSignatureVerifierTests
{
    [Fact]
    public void ComputeHmacSha256Signature_WithValidInputs_ReturnsSignature()
    {
        var timestamp = "1234567890";
        var secret = "my-secret";
        
        var result = WebhookSignatureVerifier.ComputeHmacSha256Signature(timestamp, secret);
        
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void VerifyHmacSha256Signature_WithValidSignature_ReturnsTrue()
    {
        var timestamp = "1234567890";
        var secret = "my-secret";
        var signature = WebhookSignatureVerifier.ComputeHmacSha256Signature(timestamp, secret);
        
        var result = WebhookSignatureVerifier.VerifyHmacSha256Signature(timestamp, signature, secret);
        
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyHmacSha256Signature_WithInvalidSignature_ReturnsFalse()
    {
        var timestamp = "1234567890";
        var secret = "my-secret";
        var wrongSignature = "invalid-signature";
        
        var result = WebhookSignatureVerifier.VerifyHmacSha256Signature(timestamp, wrongSignature, secret);
        
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyHmacSha256Signature_WithWrongSecret_ReturnsFalse()
    {
        var timestamp = "1234567890";
        var secret = "my-secret";
        var wrongSecret = "wrong-secret";
        var signature = WebhookSignatureVerifier.ComputeHmacSha256Signature(timestamp, secret);
        
        var result = WebhookSignatureVerifier.VerifyHmacSha256Signature(timestamp, signature, wrongSecret);
        
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyHmacSha256Signature_IsCaseInsensitive()
    {
        var timestamp = "1234567890";
        var secret = "my-secret";
        var signature = WebhookSignatureVerifier.ComputeHmacSha256Signature(timestamp, secret).ToUpperInvariant();
        
        var result = WebhookSignatureVerifier.VerifyHmacSha256Signature(timestamp, signature, secret);
        
        result.Should().BeTrue();
    }

    [Fact]
    public void ComputeSha256Signature_WithValidInputs_ReturnsSignatureWithPrefix()
    {
        var payload = "{\"test\": \"data\"}";
        var secret = "my-secret";
        
        var result = WebhookSignatureVerifier.ComputeSha256Signature(payload, secret);
        
        result.Should().StartWith("sha256=");
    }

    [Fact]
    public void VerifySha256Signature_WithValidSignature_ReturnsTrue()
    {
        var payload = "{\"test\": \"data\"}";
        var secret = "my-secret";
        var signature = WebhookSignatureVerifier.ComputeSha256Signature(payload, secret);
        
        var result = WebhookSignatureVerifier.VerifySha256Signature(payload, signature, secret);
        
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifySha256Signature_WithInvalidSignature_ReturnsFalse()
    {
        var payload = "{\"test\": \"data\"}";
        var secret = "my-secret";
        var wrongSignature = "sha256=invalid";
        
        var result = WebhookSignatureVerifier.VerifySha256Signature(payload, wrongSignature, secret);
        
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifySha256Signature_WithWrongSecret_ReturnsFalse()
    {
        var payload = "{\"test\": \"data\"}";
        var secret = "my-secret";
        var wrongSecret = "wrong-secret";
        var signature = WebhookSignatureVerifier.ComputeSha256Signature(payload, secret);
        
        var result = WebhookSignatureVerifier.VerifySha256Signature(payload, signature, wrongSecret);
        
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifySha256Signature_WithoutPrefix_ReturnsFalse()
    {
        var payload = "{\"test\": \"data\"}";
        var secret = "my-secret";
        var signature = WebhookSignatureVerifier.ComputeSha256Signature(payload, secret);
        var signatureWithoutPrefix = signature.Substring(7);
        
        var result = WebhookSignatureVerifier.VerifySha256Signature(payload, signatureWithoutPrefix, secret);
        
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifySha256Signature_IsCaseInsensitive()
    {
        var payload = "{\"test\": \"data\"}";
        var secret = "my-secret";
        var signature = WebhookSignatureVerifier.ComputeSha256Signature(payload, secret).ToUpperInvariant();
        
        var result = WebhookSignatureVerifier.VerifySha256Signature(payload, signature, secret);
        
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifySha256Signature_WithDifferentPayload_ReturnsFalse()
    {
        var payload = "{\"test\": \"data\"}";
        var differentPayload = "{\"test\": \"different\"}";
        var secret = "my-secret";
        var signature = WebhookSignatureVerifier.ComputeSha256Signature(payload, secret);
        
        var result = WebhookSignatureVerifier.VerifySha256Signature(differentPayload, signature, secret);
        
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void VerifyHmacSha256Signature_WithEmptyInputs_ReturnsFalse(string emptyValue)
    {
        var result = WebhookSignatureVerifier.VerifyHmacSha256Signature(emptyValue, "signature", "secret");
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void VerifySha256Signature_WithEmptyPayload_ReturnsFalse(string emptyPayload)
    {
        var result = WebhookSignatureVerifier.VerifySha256Signature(emptyPayload, "sha256=abc", "secret");
        result.Should().BeFalse();
    }
}
