using Microsoft.Extensions.Caching.Memory;

namespace Task_2EF.Configuration
{
    public class CacheEntryConfig : MemoryCacheEntryOptions
    {
        public MemoryCacheEntryOptions options;
        public CacheEntryConfig()
        {

        }
    }
}
