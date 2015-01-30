using Termine.Promises.ExectionControlWithRedis.Interfaces;
using Termine.Promises.Generics;
using TestConsumePromise.Properties;

namespace TestConsumePromise
{
    public class TestPromiseWorkload: GenericWorkload, ISupportRedis
    {
        public string Result { get; set; }
        public string RedisConnectionString { get { return Settings.Default.Redis; }
            set { }
        }
    }
}