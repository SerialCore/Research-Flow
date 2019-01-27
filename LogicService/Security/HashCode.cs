using System;
using System.Security.Cryptography;
using System.Text;

namespace LogicService.Security
{
    public class HashEncode
    {

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

    }
}
