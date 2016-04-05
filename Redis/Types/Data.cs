using System;
using System.Collections.Generic;

namespace Redis
{
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

        public SomeInternalType AsSmpleObject { get; set; }
        public IList<SomeInternalType> AsIList { get; set; }
    }

    [Serializable]
    public class SomeInternalType
    {
        public string Name { get; set; } = "Test-internal";
        public Guid Id { get; set; } = Guid.NewGuid();
        public List<DateTime> List { get; set; } = new List<DateTime> {DateTime.Today, DateTime.MinValue, DateTime.MaxValue};
        public int Count { get; set; } = DateTime.Now.Millisecond;
    }
}