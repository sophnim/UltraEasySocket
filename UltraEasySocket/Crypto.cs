using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace UltraEasySocket
{
    public class Crypto
    {
        public static string CreateRandomString32(int seed)
        {
            var random = new Random(seed);
            string chars = "0123456789abcdefghijklmnopqrstuvwxyz";
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < 32; ++i)
                builder.Append(chars[random.Next(chars.Length)]);

            return builder.ToString();
        }

        public static void CreatePublicKeyAndPrivateKey(out string publicKey, out string privateKey)
        {
            var rsa = new RSACryptoServiceProvider();
            var privateKeyParam = RSA.Create().ExportParameters(true);
            rsa.ImportParameters(privateKeyParam);
            privateKey = rsa.ToXmlString(true);

            var publicKeyParam = new RSAParameters();
            publicKeyParam.Modulus = privateKeyParam.Modulus;
            publicKeyParam.Exponent = privateKeyParam.Exponent;
            rsa.ImportParameters(publicKeyParam);
            publicKey = rsa.ToXmlString(false);
        }

        public static string RSAEncrypt(string getValue, string pubKey)
        {
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(pubKey);

            byte[] inbuf = (new UTF8Encoding()).GetBytes(getValue);
            byte[] encbuf = rsa.Encrypt(inbuf, false);

            return Convert.ToBase64String(encbuf);
        }

        public static string RSADecrypt(string getValue, string priKey)
        {
            var rsa = new RSACryptoServiceProvider();

            rsa.FromXmlString(priKey);
            byte[] srcbuf = Convert.FromBase64String(getValue);
            byte[] decbuf = rsa.Decrypt(srcbuf, false);

            string sDec = (new UTF8Encoding()).GetString(decbuf, 0, decbuf.Length);
            return sDec;
        }

        // key size must 32bytes
        public static byte[] AESEncrypt(byte[] toEncryptArray, string key)
        {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key);
            using (RijndaelManaged rDel = new RijndaelManaged())
            {
                rDel.Key = keyArray;
                rDel.Mode = CipherMode.ECB;
                rDel.Padding = PaddingMode.PKCS7;
                using (ICryptoTransform cTransform = rDel.CreateEncryptor())
                {
                    byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                    return resultArray;
                }
            }
        }

        public static byte[] AESDecrypt(byte[] toEncryptArray, string key)
        {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key);
            using (RijndaelManaged rDel = new RijndaelManaged())
            {
                rDel.Key = keyArray;
                rDel.Mode = CipherMode.ECB;
                rDel.Padding = PaddingMode.PKCS7;
                using (ICryptoTransform cTransform = rDel.CreateDecryptor())
                {
                    byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                    return resultArray;
                }
            }
        }

        public static byte[] SimpleXorEncrypt(byte[] toEncryptArray, string key)
        {
            byte[] workspace = new byte[toEncryptArray.Length];
            Buffer.BlockCopy(toEncryptArray, 0, workspace, 0, toEncryptArray.Length);

            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key);
            int kalen = keyArray.Length;
            int index = 0, keyindex = 0;

            foreach (var b in workspace)
            {
                workspace[index] = (byte)((byte)b ^ (byte)keyArray[keyindex]);
                index++;
                keyindex++;
                if (keyindex >= kalen)
                {
                    keyindex = 0;
                }
            }

            return workspace;
        }
    }
}
