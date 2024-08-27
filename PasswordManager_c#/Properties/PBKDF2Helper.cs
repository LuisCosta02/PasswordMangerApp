using System;
using System.Security.Cryptography;

public class PBKDF2Helper
{
    public static byte[] DeriveKey(string password, byte[] salt, int iterations, int keySize)
    {
        using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, iterations))
        {
            return rfc2898DeriveBytes.GetBytes(keySize);
        }
    }
}

