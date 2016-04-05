namespace Redis
{
    public class TestDataResult
    {
        public string SerializerType { get; set; }
        public long? Milliseconds { get; set; }
        public int ReadPerWriteRatio { get; set; } = 1;
        public int ItemsInList { get; set; }
    }
}