using System.Security.Cryptography;
using System.Text;

namespace TornBounty.Services;

/// <summary>
/// Persists the API key in %LocalAppData%\TornBounty, encrypted with Windows DPAPI
/// so only the current Windows user can decrypt it.
/// </summary>
public static class ApiKeyStore
{
    private static readonly string KeyFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TornBounty",
        "apikey.dat");

    public static string? Load()
    {
        try
        {
            if (!File.Exists(KeyFilePath))
                return null;

            var encrypted = File.ReadAllBytes(KeyFilePath);
            var decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or CryptographicException)
        {
            // Unreadable or from another user/machine; treat as no saved key
            return null;
        }
    }

    public static void Save(string apiKey)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(KeyFilePath)!);
        var encrypted = ProtectedData.Protect(Encoding.UTF8.GetBytes(apiKey), null, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(KeyFilePath, encrypted);
    }

    public static void Clear()
    {
        if (File.Exists(KeyFilePath))
            File.Delete(KeyFilePath);
    }
}
