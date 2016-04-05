using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Redis.R;
using Redis.Types;
using StackExchange.Redis;

namespace Redis
{
    internal class Program
    {
        private static IDatabase _cache;

        private static void Main()
        {
            _cache = RedisConnectorHelper.Connection.GetDatabase();
            var tester = new Tester(_cache);
            var x = tester.Test(1, GenerateList(1).ToArray()).ToList();

            Thread.Sleep(1000);

            var ratios = new[] {1, 10, 50, 100, 500, 1000, 1500, 2500};
            var noOfItems = new[] {1, 10, 25, 50, 100, 150, 200, 250, 300, 350, 400, 450, 500};

            var result = new List<TestDataResult>();
            foreach (var ratio in ratios)
                foreach (var listCount in noOfItems)
                    result.AddRange(tester.Test(ratio, GenerateList(listCount).ToArray()));

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
                Dob7 = DateTime.Today,
                AsSmpleObject = new SomeInternalType(),
                AsIList = Enumerable.Repeat(new SomeInternalType(), 10).ToList()
            };
            var list = Enumerable.Repeat(item, count).ToList();
            list.ForEach(r =>
            {
                r.Id = Guid.NewGuid();
                r.AsSmpleObject.Id = Guid.NewGuid();
                r.AsIList.ToList().ForEach(q => q.Id = Guid.NewGuid());
            });
            return list;
        }

        private static void GenerateRDataAndScripts(IEnumerable<TestDataResult> result)
        {
            var x = result
                .GroupBy(p => new {p.ReadPerWriteRatio, p.ItemsInList})
                .Select(p => new
                {
                    p.Key.ReadPerWriteRatio,
                    p.Key.ItemsInList,
                    Header = string.Join("\t", p.OrderBy(q => q.SerializerType).Select(q => q.SerializerType)),
                    Values = string.Join("\t", p.OrderBy(q => q.SerializerType).Select(q => q.Milliseconds))
                })
                .OrderBy(p => p.ReadPerWriteRatio)
                .ThenBy(p => p.ItemsInList)
                .ToList();
            const string path = @"D:\Projects\Redis\Redis\R\Data";
            x.GroupBy(p => p.ReadPerWriteRatio)
                .ToList()
                .ForEach(p =>
                {
                    var header = p.First().Header;
                    var text = header + "\r\n" + string.Join("\r\n", p.OrderBy(q => q.ItemsInList).Select(q => q.Values));
                    File.WriteAllText(Path.Combine(path, p.Key + ".dat"), text);
                });
        }
    }
}