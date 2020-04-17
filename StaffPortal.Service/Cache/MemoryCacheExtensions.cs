using Microsoft.Extensions.Caching.Memory;
using System;

namespace StaffPortal.Service.Cache
{
    public static class MemoryCacheExtensions
    {
        private static readonly object syncObject = new object();

        public static T Get<T>(this IMemoryCache memoryCache, string key, Func<T> load)
        {
            lock (syncObject)
            {
                if (memoryCache.TryGetValue(key, out T value))
                {
                    return value;
                }
                else
                {
                    value = load();

                    if (value != null) memoryCache.Set(key, value);

                    return value;
                }
            }
        }
    }
}
