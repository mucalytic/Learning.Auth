using System.Security.Cryptography;
using System.Text;

namespace Learning.Auth.Identity.Cookies.Api;

public static class StringExtensions
{
    public static string Hash(this string value) =>
        Convert.ToBase64String(MD5.HashData(Encoding.UTF8.GetBytes(value)));
}
