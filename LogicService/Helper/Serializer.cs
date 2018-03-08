using System;
using System.IO;
using System.Runtime.Serialization;

namespace LogicService.Helper
{
    public static class Serializers
    {

        public static byte[] Serialize<T>(T obj)
        {
            MemoryStream stream = new MemoryStream();
            DataContractSerializer dcs = new DataContractSerializer(typeof(T));
            dcs.WriteObject(stream, obj);
            return stream.ToArray();
        }

        public static T Deserialize<T>(byte[] buffer)
        {
            MemoryStream stream = new MemoryStream(buffer);
            DataContractSerializer dcs = new DataContractSerializer(typeof(T));
            return (T)dcs.ReadObject(stream);
        }

    }
}
