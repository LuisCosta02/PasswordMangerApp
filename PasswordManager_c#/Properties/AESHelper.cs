using System.Security.Cryptography;

public class AESHelper
{
    private readonly byte[] _key;

    public AESHelper(byte[] key)
    {
        if (key == null || key.Length != 32) // AES-256 requires a 256-bit key (32 bytes)
        {
            throw new ArgumentException("Key must be 32 bytes long.");
        }
        _key = key;
    }

    public (string EncryptedData, string IV) Encrypt(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = _key;
            aes.GenerateIV();
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return (Convert.ToBase64String(ms.ToArray()), Convert.ToBase64String(aes.IV));
                }
            }
        }
    }

    public string Encrypt(string plainText, string ivBase64)
    {
        byte[] iv = Convert.FromBase64String(ivBase64);

        using (Aes aes = Aes.Create())
        {
            aes.Key = _key;
            aes.IV = iv;
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
    }

    public string Decrypt(string encryptedData, string ivBase64)
    {
        byte[] iv = Convert.FromBase64String(ivBase64);
        byte[] buffer = Convert.FromBase64String(encryptedData);

        using (Aes aes = Aes.Create())
        {
            aes.Key = _key;
            aes.IV = iv;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream(buffer))
            {
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }
}
