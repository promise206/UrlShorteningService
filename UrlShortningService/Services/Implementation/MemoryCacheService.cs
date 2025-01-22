using Microsoft.Extensions.Caching.Memory;
using UrlShortningService.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace UrlShortningService.Services.Implementation
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        // Set a cache entry with expiration time
        public async Task SetAsync(string key, string value, TimeSpan expiration)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };
            _memoryCache.Set(key, value, cacheOptions);
            await Task.CompletedTask; // Return completed task as this is synchronous
        }

        // Get a cache entry by key
        public async Task<string> GetAsync(string key)
        {
            if (_memoryCache.TryGetValue(key, out string value))
            {
                return await Task.FromResult(value); // Return cached value if it exists
            }

            return null; // Return null if no cache entry exists
        }

        // Remove a cache entry
        public async Task RemoveAsync(string key)
        {
            _memoryCache.Remove(key);
            await Task.CompletedTask; // Return completed task as this is synchronous
        }
    }
}
