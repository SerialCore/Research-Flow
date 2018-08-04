using System;
using System.Security.Cryptography;
using System.Text;

namespace LogicService.Security
{
    /// <summary>
    /// Encrypt 的摘要说明。
    /// </summary>
    public class DEncrypt
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        public DEncrypt()
        {

        }

        #region 加密/解密string
        /// <summary>
        /// 加密string
        /// </summary>
        /// <param name="original">明文</param>
        /// <param name="key">密钥</param>
        /// <returns>密文</returns>
        public static string Encrypt(string original, string key)
        {
            if (key == null)
                key = "blamder";
            byte[] buff = System.Text.Encoding.Default.GetBytes(original);
            byte[] kb = System.Text.Encoding.Default.GetBytes(key);
            return Convert.ToBase64String(Encrypt(buff, kb));
        }
        /// <summary>
        /// 解密string
        /// </summary>
        /// <param name="original">密文</param>
        /// <param name="key">密钥</param>
        /// <returns>明文</returns>
        public static string Decrypt(string original, string key)
        {
            if (key == null)
                key = "blamder";
            return Decrypt(original, key, System.Text.Encoding.Default);
        }

        /// <summary>
        /// 使用给定密钥字符串解密string,返回指定编码方式明文
        /// </summary>
        /// <param name="encrypted">密文</param>
        /// <param name="key">密钥</param>
        /// <param name="encoding">字符编码方案</param>
        /// <returns>明文</returns>
        public static string Decrypt(string encrypted, string key, Encoding encoding)
        {
            byte[] buff = Convert.FromBase64String(encrypted);
            byte[] kb = System.Text.Encoding.Default.GetBytes(key);
            return encoding.GetString(Decrypt(buff, kb));
        }
        #endregion

        #region  加密/解密/byte[]

        /// <summary>
        /// 加密数据
        /// </summary>
        /// <param name="original">明文</param>
        /// <param name="key">密钥</param>
        /// <returns>密文</returns>
        public static byte[] Encrypt(byte[] original, byte[] key = null)
        {
            if (key == null)
                key = System.Text.Encoding.Default.GetBytes("blamder");
            TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
            des.Key = MakeMD5(key);
            des.Mode = CipherMode.ECB;

            return des.CreateEncryptor().TransformFinalBlock(original, 0, original.Length);
        }

        /// <summary>
        /// 解密数据
        /// </summary>
        /// <param name="encrypted">密文</param>
        /// <param name="key">密钥</param>
        /// <returns>明文</returns>
        public static byte[] Decrypt(byte[] encrypted, byte[] key = null)
        {
            if (key == null)
                key = System.Text.Encoding.Default.GetBytes("blamder");
            TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
            des.Key = MakeMD5(key);
            des.Mode = CipherMode.ECB;

            return des.CreateDecryptor().TransformFinalBlock(encrypted, 0, encrypted.Length);
        }

        #endregion

        #region HashCode

        public static string GetRandomValue()
        {
            Random Seed = new Random();
            string RandomVaule = Seed.Next(1, int.MaxValue).ToString();
            return RandomVaule;
        }

        public static byte[] MakeMD5(byte[] original)
        {
            MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
            byte[] Value = hashmd5.ComputeHash(original);
            hashmd5 = null;
            return Value;
        }

        public static string MakeMD5(string Security)
        {
            byte[] Message = Encoding.Default.GetBytes(Security);
            MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
            byte[] Value = hashmd5.ComputeHash(Message);
            hashmd5 = null;
            Security = "";
            foreach (byte o in Value)
            {
                Security += o.ToString("x2");
            }
            return Security;
        }

        public static string MakeSHA512(string Security)
        {
            byte[] Message = Encoding.Default.GetBytes(Security);
            SHA512Managed Arithmetic = new SHA512Managed();
            byte[] Value = Arithmetic.ComputeHash(Message);
            Security = "";
            foreach (byte o in Value)
            {
                Security += o.ToString("x2");
            }
            return Security;
        }

        #endregion
    }
}