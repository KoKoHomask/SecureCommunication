using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace SecureCommunication.Common
{
    public class RSAHelper
    {
        public RSAEncryptionPadding EncryptionPadding { get; set; } = RSAEncryptionPadding.Pkcs1;
        public RSACryptoServiceProvider RSA { get; private set; }
        public string KeyToXmlString { get => ToXmlString(false); }
        public string KeyToXmlStringWithPrivate { get => ToXmlString(true); }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="KeySizeInBits">RSA位数 2048,7680等</param>
        public RSAHelper(int KeySizeInBits)
        {
            RSA = new RSACryptoServiceProvider(KeySizeInBits);
        }
        public RSAHelper()
        {
            RSA = new RSACryptoServiceProvider();
        }
        public RSAHelper(string xmlkey)
        {
            RSA = new RSACryptoServiceProvider();
            FromXmlString(xmlkey);
        }
        void FromXmlString(string xmlString)
        {
            RSAParameters parameters = new RSAParameters();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);
            if (xmlDoc.DocumentElement.Name.Equals("RSAKeyValue"))
            {
                foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "Modulus": parameters.Modulus = Convert.FromBase64String(node.InnerText); break;
                        case "Exponent": parameters.Exponent = Convert.FromBase64String(node.InnerText); break;
                        case "P": parameters.P = Convert.FromBase64String(node.InnerText); break;
                        case "Q": parameters.Q = Convert.FromBase64String(node.InnerText); break;
                        case "DP": parameters.DP = Convert.FromBase64String(node.InnerText); break;
                        case "DQ": parameters.DQ = Convert.FromBase64String(node.InnerText); break;
                        case "InverseQ": parameters.InverseQ = Convert.FromBase64String(node.InnerText); break;
                        case "D": parameters.D = Convert.FromBase64String(node.InnerText); break;
                    }
                }
            }
            else
            {
                throw new Exception("Invalid XML RSA key.");
            }

            RSA.ImportParameters(parameters);
        }
        string ToXmlString(bool includePrivateParameters)
        {
            RSAParameters parameters = RSA.ExportParameters(includePrivateParameters);

            if (includePrivateParameters)
            {
                return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent><P>{2}</P><Q>{3}</Q><DP>{4}</DP><DQ>{5}</DQ><InverseQ>{6}</InverseQ><D>{7}</D></RSAKeyValue>",
                    Convert.ToBase64String(parameters.Modulus),
                    Convert.ToBase64String(parameters.Exponent),
                    Convert.ToBase64String(parameters.P),
                    Convert.ToBase64String(parameters.Q),
                    Convert.ToBase64String(parameters.DP),
                    Convert.ToBase64String(parameters.DQ),
                    Convert.ToBase64String(parameters.InverseQ),
                    Convert.ToBase64String(parameters.D));
            }
            return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
                    Convert.ToBase64String(parameters.Modulus),
                    Convert.ToBase64String(parameters.Exponent));
        }
        public byte[] Encrypt(byte[] input)
        {
            Func<byte[], byte[]> encrypt = sou =>
            {
                int maxBlockSize = RSA.KeySize / 8 - 11;
                if (sou.Length <= maxBlockSize)
                {
                    return RSA.Encrypt(sou, EncryptionPadding);
                }
                using (MemoryStream plaiStream = new MemoryStream(sou))
                {
                    using (MemoryStream crypStream = new MemoryStream())
                    {
                        byte[] buffer = new byte[maxBlockSize];
                        int blockSize = plaiStream.Read(buffer, 0, maxBlockSize);

                        while (blockSize > 0)
                        {
                            byte[] toEncrypt = new byte[blockSize];
                            Array.Copy(buffer, 0, toEncrypt, 0, blockSize);

                            byte[] cryptograph = RSA.Encrypt(toEncrypt, EncryptionPadding);
                            crypStream.Write(cryptograph, 0, cryptograph.Length);

                            blockSize = plaiStream.Read(buffer, 0, maxBlockSize);
                        }

                        return crypStream.ToArray();
                    }
                }
            };
            return MarkData(encrypt(input));
        }
        public byte[] Decrypt(byte[] input)
        {
            if (IsEncrypt(input))
            {
                Func<byte[], byte[]> decrypt = sou =>
                {

                    int maxBlockSize = RSA.KeySize / 8;

                    if (sou.Length <= maxBlockSize)
                        return RSA.Decrypt(sou, EncryptionPadding);

                    using (MemoryStream crypStream = new MemoryStream(sou))
                    {
                        using (MemoryStream plaiStream = new MemoryStream())
                        {
                            byte[] buffer = new byte[maxBlockSize];
                            int blockSize = crypStream.Read(buffer, 0, maxBlockSize);

                            while (blockSize > 0)
                            {
                                byte[] toDecrypt = new byte[blockSize];
                                Array.Copy(buffer, 0, toDecrypt, 0, blockSize);

                                byte[] plaintext = RSA.Decrypt(toDecrypt, EncryptionPadding);
                                plaiStream.Write(plaintext, 0, plaintext.Length);

                                blockSize = crypStream.Read(buffer, 0, maxBlockSize);
                            }

                            return plaiStream.ToArray();
                        }
                    }
                };
                return decrypt(ClearDataMark(input));
            }
            return input;
        }
        private byte[] MarkData(byte[] input)
        {
            byte[] newBytes = new byte[input.Length + 200];
            for (int i = 0; i < newBytes.Length; i++)
            {
                if (i < 100 || i > newBytes.Length - 100 - 1)
                {
                    newBytes[i] = 0;
                }
                else
                {
                    newBytes[i] = input[i - 100];
                }
            }
            return newBytes;
        }
        private byte[] ClearDataMark(byte[] input)
        {
            byte[] newBytes = new byte[input.Length - 200];
            for (int i = 100; i < input.Length - 100; i++)
            {
                newBytes[i - 100] = input[i];
            }
            return newBytes;
        }
        private bool IsEncrypt(byte[] input)
        {
            for (int i = 0; i < 100; i++)
            {
                if (input[i] != 0 || input[input.Length - i - 1] != 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
