using System;
using StackExchange.Redis;

namespace Redis
{
    public class RedisConnectorHelper
    {
        static RedisConnectorHelper()
        {
            LazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect("localhost"));
        }

        private static readonly Lazy<ConnectionMultiplexer> LazyConnection;

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return LazyConnection.Value;
            }
        }
    }
}