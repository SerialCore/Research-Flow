using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace LogicService.Helper
{
    public class SerializeHelper
    {

        #region byte[]

        // Stream is a raw and unformatted bit arrry.
        // Do not use Encoding.Default.
        // Use UTF-8 for languages supporting.

        public static byte[] StringToBytes(string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }

        public static string BytesToJson(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public static byte[] SerializeToBytes(object o)
        {
            IFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, o);
                stream.Position = 0;
                return stream.ToArray();
            }
        }

        public static T DeserializeFromBytes<T>(byte[] bytes) where T : class
        {
            IFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }

        #endregion

        #region Json

        public static string SerializeToJson(object o)
        {
            return JsonConvert.SerializeObject(o);
        }

        public static T DeserializeJsonToObject<T>(string json) where T : class
        {
            JsonSerializer serializer = new JsonSerializer();
            StringReader sr = new StringReader(json);
            object o = serializer.Deserialize(new JsonTextReader(sr), typeof(T));
            T t = o as T;
            return t;
        }

        public static List<T> DeserializeJsonToList<T>(string json) where T : class
        {
            JsonSerializer serializer = new JsonSerializer();
            StringReader sr = new StringReader(json);
            object o = serializer.Deserialize(new JsonTextReader(sr), typeof(List<T>));
            List<T> list = o as List<T>;
            return list;
        }

        public static T DeserializeAnonymousType<T>(string json, T anonymousTypeObject) where T : class
        {
            T t = JsonConvert.DeserializeAnonymousType(json, anonymousTypeObject);
            return t;
        }

        #endregion

        #region BinaryFormatter

        public static string SerializeToBinary(object o)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                formatter.Serialize(ms, o);
                ms.Position = 0;
                byte[] bytes = ms.ToArray();
                StringBuilder sb = new StringBuilder();
                foreach (byte bt in bytes)
                {
                    sb.Append(string.Format("{0:X2}", bt));
                }
                return sb.ToString();
            }
        }

        public static T DeserializeFromBinary<T>(string str) where T : class
        {
            int intLen = str.Length / 2;
            byte[] bytes = new byte[intLen];
            for (int i = 0; i < intLen; i++)
            {
                int ibyte = Convert.ToInt32(str.Substring(i * 2, 2), 16);
                bytes[i] = (byte)ibyte;
            }
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                return (T)formatter.Deserialize(ms);
            }
        }

        #endregion

    }
}
