using System.Security.Cryptography;
using System.Buffers.Text;
using System.Text;

namespace Learning.Auth.OAuth.Custom.Server.Services;

public class ValidationCodeVerifier
{
    public bool ValidateS256(string codeVerifier, AuthCode authCode)
    {
        using var sha256 = SHA256.Create();
        var codeVerifierBytes = Encoding.ASCII.GetBytes(codeVerifier);
        var codeVerifierHash = sha256.ComputeHash(codeVerifierBytes);
        var codeVerifierBase64 = Base64Url.EncodeToString(codeVerifierHash);
        return codeVerifierBase64 == authCode.CodeChallenge;
    }
}
