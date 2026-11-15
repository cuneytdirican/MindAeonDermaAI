using Microsoft.AspNetCore.DataProtection;
using MindAeonDermaAI.Core.Ai;

namespace MindAeonDermaAI.Infrastructure.Ai;

/// <summary>Encrypts provider API keys at rest using ASP.NET Data Protection.</summary>
public class SecretProtector : ISecretProtector
{
    private readonly IDataProtector _protector;

    public SecretProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("MindAeonDermaAI.ApiKeys.v1");
    }

    public string Encrypt(string plaintext) => _protector.Protect(plaintext);

    public string Decrypt(string ciphertext)
    {
        try { return _protector.Unprotect(ciphertext); }
        catch { return string.Empty; }
    }
}
