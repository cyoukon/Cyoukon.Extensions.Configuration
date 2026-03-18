namespace Cyoukon.Extensions.Configuration.Abstractions.Services
{
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }
}
