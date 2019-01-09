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
        private int keySizeInBits = 7680;
        private RSAEncryptionPadding rSAEncryptionPadding = RSAEncryptionPadding.Pkcs1;//.OaepSHA512;
        private int encryptBufferSizeLess = 130;
        public RSA RSA { get; private set; }
        public string KeyToXmlString { get => ToXmlString(false); }
        public string KeyToXmlStringWithPrivate { get => ToXmlString(true); }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="KeySizeInBits">RSA位数 2048,7680等</param>
        /// <param name="EncryptBufferSizeLess">每次加密数组比最大限制小多少</param>
        /// <param name="Padding">方向</param>
        public RSAHelper(int KeySizeInBits, int EncryptBufferSizeLess, RSAEncryptionPadding Padding)
        {
            keySizeInBits = KeySizeInBits;
            encryptBufferSizeLess = EncryptBufferSizeLess;
            rSAEncryptionPadding = Padding;
            RSA = RSA.Create(keySizeInBits);
        }
        public RSAHelper()
        {
            RSA = RSA.Create(keySizeInBits);
        }
        public RSAHelper(string xmlkey)
        {
            RSA = RSA.Create();
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
        public byte[] Encrypt(byte[] encryptArray)
        {
            var bufferSize = RSA.KeySize / 8 - encryptBufferSizeLess;
            var buffer = new byte[bufferSize];
            using (MemoryStream inputStream = new MemoryStream(encryptArray),
                     outputStream = new MemoryStream())
            {
                while (true)
                {
                    int readSize = inputStream.Read(buffer, 0, bufferSize);
                    if (readSize <= 0)
                    {
                        break;
                    }

                    var temp = new byte[readSize];
                    Array.Copy(buffer, 0, temp, 0, readSize);
                    var encryptedBytes = RSA.Encrypt(temp, rSAEncryptionPadding);
                    outputStream.Write(encryptedBytes, 0, encryptedBytes.Length);
                }
                return outputStream.ToArray();
            }
        }
        public byte[] Decrypt(byte[] decryptArray)
        {
            int bufferSize = RSA.KeySize / 8;
            var buffer = new byte[bufferSize];
            using (MemoryStream inputStream = new MemoryStream(decryptArray),
                 outputStream = new MemoryStream())
            {
                while (true)
                {
                    int readSize = inputStream.Read(buffer, 0, bufferSize);
                    if (readSize <= 0)
                    {
                        break;
                    }
                    var temp = new byte[readSize];
                    Array.Copy(buffer, 0, temp, 0, readSize);
                    var rawBytes = RSA.Decrypt(temp, rSAEncryptionPadding);
                    outputStream.Write(rawBytes, 0, rawBytes.Length);
                }
                return outputStream.ToArray();
            }
        }
    }
}
