using System.Security.Cryptography;
using System.Text;

namespace slender_server.Infra.Auth;

internal static class TokenHasher
{
    public static string Hash(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash  = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant(); // 64-char fixed-length string
    }
}