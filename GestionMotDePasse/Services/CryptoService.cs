using System.Security.Cryptography;

namespace GestionMotDePasse.Services;

public static class CryptoService
{
	public static string Encrypt(string plainText, byte[] key)
	{
		using var aes = Aes.Create();
        using var encryptor = aes.CreateEncryptor(key, aes.IV);
        using var memoryStream = new MemoryStream();
        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write)) 
        { 
            using var streamWriter = new StreamWriter(cryptoStream);
            streamWriter.Write(plainText);
        }

        var encryptedText = memoryStream.ToArray();
        var IV = aes.IV;
        var result = new byte[aes.IV.Length + encryptedText.Length];

        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(encryptedText, 0, result, aes.IV.Length, encryptedText.Length);

        return Convert.ToBase64String(result);
    }

    public static string Decrypt(string EncryptedText, byte[] key)
    {
        var fullCipher = Convert.FromBase64String(EncryptedText);
      
        var iv = new byte[16];
        var cipher = new byte[16];
        Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

        using var aes = Aes.Create();
        using var decryptor = aes.CreateDecryptor(key, iv);
        using var memoryStream = new MemoryStream(cipher);
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using var streamReader = new StreamReader(cryptoStream);
        
        return streamReader.ReadToEnd();
    }
}
