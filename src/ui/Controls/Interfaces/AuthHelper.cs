using System;
using System.Security.Cryptography;

using System.Text;

namespace Nikse.SubtitleEdit.Controls.Interfaces
{
    public static class AuthHelper
{
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("3F2504E0-4B99-41D3-9B0C-0305E82C3304");

    public static void SaveAuthData(string clientID, string clientSecret)
    {
        var encryptedPassword = Protect(clientSecret);
        Properties.Settings.Default.ClientID = clientID;
        Properties.Settings.Default.ClientSecret = encryptedPassword;
        Properties.Settings.Default.Save();
    }

    public static (string ClientID, string ClientSecret) LoadAuthData()
    {
        var clientID = Properties.Settings.Default.ClientID;
        var encryptedClientSecret = Properties.Settings.Default.ClientSecret;
        var clientSecret = !string.IsNullOrWhiteSpace(encryptedClientSecret) ? Unprotect(encryptedClientSecret) : null;
        return (clientID, clientSecret);
    }

    private static string Protect(string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        var encryptedBytes = ProtectedData.Protect(bytes, Entropy, DataProtectionScope.CurrentUser);
   
        return Convert.ToBase64String(encryptedBytes);
    }

    private static string Unprotect(string encryptedData)
    {
        var bytes = Convert.FromBase64String(encryptedData);
        var decryptedBytes = ProtectedData.Unprotect(bytes, Entropy, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
}