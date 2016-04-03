using MsgPack.Serialization;
using NetSerializer;
using Salar.Bois;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Redis
{
    class Program
    {
        static IDatabase cache;
        static void Main(string[] args)
        {
            report = new StringBuilder();
            
            cache = RedisConnectorHelper.Connection.GetDatabase();
            Test(1, GenerateList(1));

            var ratios = new[] { 1, 10, 50, 100, 500, 1000, 5000, 10000 };
            var noOfItems = new[] { 1, 10, 25, 50, 100, 150, 200, 250, 300, 350, 400, 450, 500};

            foreach (var ratio in ratios)
                foreach (var listCount in noOfItems)
                    Test(ratio, GenerateList(listCount));

            File.WriteAllText("report.txt", report.ToString());
        }

        private static List<Data> GenerateList(int count)
        {
            var item = new Data
            {
                Id = Guid.NewGuid(),
                Name = "cez\"ar",
                Dob = DateTime.Today,
                Name1 = "cezar",
                Dob1 = DateTime.Today,
                Name2 = "cezar",
                Dob2 = DateTime.Today,
                Name3 = "cezar",
                Dob3 = DateTime.Today,
                Name4 = "cezar",
                Dob4 = DateTime.Today,
                Name5 = "cezar",
                Dob5 = DateTime.Today,
                Name6 = "cezar",
                Dob6 = DateTime.Today,
                Name7 = "cezar",
                Dob7 = DateTime.Today
            };
            var list = Enumerable.Repeat(item, count).ToList();
            list.ForEach(r => r.Id = Guid.NewGuid());
            return list;
        }

        private static void Test<T>(int ratio, T item) where T : class
        {
            var count = (item as List<Data>).Count();
            Console.WriteLine($"==========Ratio: {ratio} Count: {count} =================");
            report.AppendFormat($"==========Ratio: {ratio} Count: {count} =================").AppendLine();
            TestSingleObject(
                p => Serializers.NewtonSoftJsonSerialize(p),
                p => Serializers.NewtonSoftJsonDeserialize<T>(p),
                item,
                ratio,
                "NewtonSoftJson");

            TestSingleObject(
                p => Serializers.JilSerialize(p),
                p => Serializers.JilDeserialize<T>(p),
                item,
                ratio,
                "JIL");

            var netSerializer = new Serializer(new[] { typeof(T) });
            TestSingleObject(
                p => Serializers.NetSerialize(netSerializer, p),
                p => Serializers.NetDeserialize<T>(netSerializer, p),
                item,
                ratio,
                "NET");

            var boisSerializer = new BoisSerializer();
            TestSingleObject(
                p => Serializers.BoisSerialize(boisSerializer, p),
                p => Serializers.BoisDeserialize<T>(boisSerializer, p),
                item,
                ratio,
                "Bois");

            var msgSerializer = SerializationContext.Default.GetSerializer<T>();
            TestSingleObject(
                p => Serializers.MsgPackSerialize(msgSerializer, p),
                p => Serializers.MsgPackDeserialize<T>(msgSerializer, p),
                item,
                ratio,
                "MessagePack");

            TestSingleObject(
                p => Serializers.NetJsonSerialize(p),
                p => Serializers.NetJsonDeserialize<T>(p),
                item,
                ratio,
                "NetJson");
        }

        public static void TestSingleObject<T>(
            Func<T, RedisValue> serialize,
            Func<RedisValue, T> deserialize,
            T item,
            int readWriteRatio,
            string testName)
        {
            var s = Stopwatch.StartNew();
            cache.StringSet(testName, serialize(item), expiry: TimeSpan.FromSeconds(30000));
            for (var j = 0; j < readWriteRatio; j++)
            {
                if (cache.KeyExists(testName))
                {
                    var data = cache.StringGet(testName);
                    var x = deserialize(data);
                }

            }
            report.AppendFormat($"{testName}: {s.ElapsedMilliseconds}ms").AppendLine();
            Console.WriteLine($"{testName}: {s.ElapsedMilliseconds}ms");
        }

        public static StringBuilder report; 
    }


    public class RedisConnectorHelper
    {
        static RedisConnectorHelper()
        {
            RedisConnectorHelper.lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                return ConnectionMultiplexer.Connect("localhost");
            });
        }

        private static Lazy<ConnectionMultiplexer> lazyConnection;

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }
    }

    [Serializable]
    public class Data
    {
        public string Name { get; set; }

        public DateTime? Dob { get; set; }
        public string Name1 { get; set; }

        public DateTime? Dob1 { get; set; }
        public string Name2 { get; set; }

        public DateTime? Dob2 { get; set; }
        public string Name3 { get; set; }

        public DateTime? Dob3 { get; set; }
        public string Name4 { get; set; }

        public DateTime? Dob4 { get; set; }
        public string Name5 { get; set; }

        public DateTime? Dob5 { get; set; }
        public string Name6 { get; set; }

        public DateTime? Dob6 { get; set; }
        public string Name7 { get; set; }

        public DateTime? Dob7 { get; set; }
        public Guid? Id { get; set; }
    }
}
