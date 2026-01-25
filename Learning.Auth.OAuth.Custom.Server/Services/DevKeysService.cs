using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Learning.Auth.OAuth.Custom.Server.Services;

public class DevKeysService
{
    public RSA            RsaKey { get; } = RSA.Create();
    public RsaSecurityKey RsaSecurityKey => new(RsaKey);
    
    public DevKeysService(IWebHostEnvironment env)
    {
        var path = Path.Combine(env.ContentRootPath, "crypto_key");
        if (File.Exists(path))
        {
            var privateKey = File.ReadAllBytes(path);
            RsaKey.ImportRSAPrivateKey(privateKey, out _);
        }
        else
        {
            var privateKey = RsaKey.ExportRSAPrivateKey();
            File.WriteAllBytes(path, privateKey);
        }
    }
}
