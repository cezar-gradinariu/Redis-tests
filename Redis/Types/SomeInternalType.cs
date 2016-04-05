using System;
using System.Collections.Generic;

namespace Redis.Types
{
    [Serializable]
    public class SomeInternalType
    {
        public string Name { get; set; } = "Test-internal";
        public Guid Id { get; set; } = Guid.NewGuid();
        public List<DateTime> List { get; set; } = new List<DateTime> {DateTime.Today, DateTime.MinValue, DateTime.MaxValue};
        public int Count { get; set; } = DateTime.Now.Millisecond;
    }
}