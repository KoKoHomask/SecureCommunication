using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecureCommunication.Common
{
    public class CustomerSecurity
    {
        public static string Encrypt(string content, string secretKey)
        {
            char[] data = content.ToCharArray();
            char[] key = secretKey.ToCharArray();
            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= key[i % key.Length];
            }
            return new string(data); //.ToString();
        }
        public static string Decrypt(string encryptStr, string secretKey)
        {
            char[] data = encryptStr.ToArray();
            char[] key = secretKey.ToCharArray();
            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= key[i % key.Length];
            }
            return new string(data);
        }
    }
}
