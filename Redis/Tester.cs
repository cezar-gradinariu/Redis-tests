using System;
using System.Collections.Generic;
using System.Diagnostics;
using MsgPack.Serialization;
using NetSerializer;
using Salar.Bois;
using StackExchange.Redis;

namespace Redis
{
    public class Tester
    {
        private readonly IDatabase _cache;

        public Tester(IDatabase cache)
        {
            _cache = cache;
        }

        public IEnumerable<TestDataResult> Test<T>(int ratio, T[] items) where T : class
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

            var netSerializer = new Serializer(new[] { typeof(T[]), typeof(SomeInternalType), typeof(IList<SomeInternalType>) });
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

        internal TestDataResult TestSingleObject<T>(
            Func<T[], RedisValue> serialize,
            Func<RedisValue, T[]> deserialize,
            T[] items,
            int readWriteRatio,
            string serializerType)
        {
            var result = new TestDataResult
            {
                ItemsInList = items.Length,
                ReadPerWriteRatio = readWriteRatio,
                SerializerType = serializerType
            };
            try
            {
                var s = Stopwatch.StartNew();
                _cache.StringSet(serializerType, serialize(items), TimeSpan.FromSeconds(30000));
                for (var j = 0; j < readWriteRatio; j++)
                {
                    if (_cache.KeyExists(serializerType))
                    {
                        var data = _cache.StringGet(serializerType);
                        var x = deserialize(data);
                    }
                }
                var totalMs = s.ElapsedMilliseconds;
                Console.WriteLine($"{serializerType}: {totalMs}ms");
                result.Milliseconds = totalMs;
            }
            catch (Exception)
            {
                Console.WriteLine("Failed");
                result.Milliseconds = -100;
            }
            return result;
        }
    }
}