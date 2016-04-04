using MsgPack.Serialization;
using NetSerializer;
using Salar.Bois;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Redis
{
    class Program
    {
        static IDatabase cache;
        static void Main(string[] args)
        {
            cache = RedisConnectorHelper.Connection.GetDatabase();
            Test(1, GenerateList(1).ToArray());

            var ratios = new[] { 1, 10, 50, 100 };//, 500, 1000, 5000, 10000 };
            var noOfItems = new[] { 1, 10, 25, 50, 100 };//, 150, 200, 250, 300, 350, 400, 450, 500};

            var result = new List<TestDataResult>();
            foreach (var ratio in ratios)
                foreach (var listCount in noOfItems)
                    result.AddRange(Test(ratio, GenerateList(listCount).ToArray()));

            GenerateRDataAndScripts(result);

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

        private static IEnumerable<TestDataResult> Test<T>(int ratio, T[] items) where T : class
        {
            Console.WriteLine($"==========Ratio: {ratio} Count: {items.Length} =================");
            yield return TestSingleObject(
                p => Serializers.NewtonSoftJsonSerialize(p),
                p => Serializers.NewtonSoftJsonDeserialize<T[]>(p),
                items,
                ratio,
                "NewtonSoftJson");

            yield return TestSingleObject(
                p => Serializers.JilSerialize(p),
                p => Serializers.JilDeserialize<T[]>(p),
                items,
                ratio,
                "JIL");

            var netSerializer = new Serializer(new[] { typeof(T[]) });
            yield return TestSingleObject(
                p => Serializers.NetSerialize(netSerializer, p),
                p => Serializers.NetDeserialize<T[]>(netSerializer, p),
                items,
                ratio,
                "NET");

            var boisSerializer = new BoisSerializer();
            yield return TestSingleObject(
                p => Serializers.BoisSerialize(boisSerializer, p),
                p => Serializers.BoisDeserialize<T[]>(boisSerializer, p),
                items,
                ratio,
                "Bois");

            var msgSerializer = SerializationContext.Default.GetSerializer<T[]>();
            yield return TestSingleObject(
                p => Serializers.MsgPackSerialize(msgSerializer, p),
                p => Serializers.MsgPackDeserialize(msgSerializer, p),
                items,
                ratio,
                "MessagePack");

            yield return TestSingleObject(
                p => Serializers.NetJsonSerialize(p),
                p => Serializers.NetJsonDeserialize<T[]>(p),
                items,
                ratio,
                "NetJson");
        }

        public static TestDataResult TestSingleObject<T>(
            Func<T[], RedisValue> serialize,
            Func<RedisValue, T[]> deserialize,
            T[] items,
            int readWriteRatio,
            string serializerType)
        {
            var s = Stopwatch.StartNew();
            cache.StringSet(serializerType, serialize(items), expiry: TimeSpan.FromSeconds(30000));
            for (var j = 0; j < readWriteRatio; j++)
            {
                if (cache.KeyExists(serializerType))
                {
                    var data = cache.StringGet(serializerType);
                    var x = deserialize(data);
                }
            }
            var totalMs = s.ElapsedMilliseconds;
            Console.WriteLine($"{serializerType}: {totalMs}ms");
            return new TestDataResult
            {
                Milliseconds = totalMs,
                ReadPerWriteRatio = readWriteRatio,
                SerializerType = serializerType,
                ItemsInList = items.Length
            };
        }

        private static void GenerateRDataAndScripts(List<TestDataResult> result)
        {
            result
                .GroupBy(p => p.ReadPerWriteRatio)
                .Select(p => string.Join("\t", p.OrderBy(q => q.SerializerType).Select(q=> q.Milliseconds))

        }

    }

    public class RedisConnectorHelper
    {
        static RedisConnectorHelper()
        {
            lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
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
    
    public class TestDataResult
    {
        public string SerializerType { get; set; }
        public long Milliseconds { get; set; }
        public int ReadPerWriteRatio { get; set; } = 1;
        public int ItemsInList { get; set; }
    }
}
