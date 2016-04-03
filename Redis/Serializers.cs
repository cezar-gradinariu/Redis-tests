using Jil;
using MsgPack.Serialization;
using NetSerializer;
using Salar.Bois;
using System.IO;
using Newtonsoft.Json;

namespace Redis
{
    public static class Serializers 
    {
        public static string NewtonSoftJsonSerialize(object a)
        {
            return JsonConvert.SerializeObject(a);
        }
        public static T NewtonSoftJsonDeserialize<T>(string a)
        {
            return JsonConvert.DeserializeObject<T>(a);
        }

        public static string JilSerialize(object a)
        {
            using (var output = new StringWriter())
            {
                JSON.Serialize(a, output);
                return output.ToString();
            }
        }
        public static T JilDeserialize<T>(string a)
        {
            using (var input = new StringReader(a))
            {
                var result = JSON.Deserialize<T>(input);
                return result;
            }
        }

        public static string NetJsonSerialize(object a)
        {
            return NetJSON.NetJSON.Serialize(a);
        }
        public static T NetJsonDeserialize<T>(string a) where T : class
        {
            return NetJSON.NetJSON.Deserialize<T>(a);
        }

        public static byte[] NetSerialize(Serializer ser, object a)
        {
            using (var output = new MemoryStream())
            {
                ser.Serialize(output, a);
                output.Position = 0;
                return output.ToArray();
            }
        }
        public static T NetDeserialize<T>(Serializer ser, byte[] a)
        {
            using (var output = new MemoryStream(a))
            {
                return (T)ser.Deserialize(output);
            }
        }

        public static byte[] BoisSerialize(BoisSerializer boisSerializer, object o)
        {
            using (var mem = new MemoryStream())
            {
                boisSerializer.Serialize(o, o.GetType(), mem);
                mem.Position = 0;
                return mem.ToArray();
            }
        }
        public static T BoisDeserialize<T>(BoisSerializer boisSerializer, byte[] data)
        {
            using (var dataStream = new MemoryStream(data))
            {
                return boisSerializer.Deserialize<T>(dataStream);
            }
        }

        public static byte[] MsgPackSerialize<T>(MessagePackSerializer<T> serializer, T o)
        {
            using (var mem = new MemoryStream())
            {
                serializer.Pack(mem, o);
                mem.Position = 0;
                return mem.ToArray();
            }
        }
        public static T MsgPackDeserialize<T>(MessagePackSerializer<T> serializer, byte[] data)
        {
            using (var dataStream = new MemoryStream(data))
            {
                //return serializer.UnpackSingleObject(data);
                return serializer.Unpack(dataStream);
            }
        }

    }
}
