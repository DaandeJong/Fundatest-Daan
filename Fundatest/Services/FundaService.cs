using Fundatest.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Fundatest.Services
{
    public class FundaService : IApiService
    {
        private const double cacheDuration = 10;

        private IMemoryCache _cache;
        private readonly IConfiguration _configuration;

        public FundaService(IMemoryCache memoryCache, IConfiguration configuration)
        {
            _cache = memoryCache;
            _configuration = configuration;
        }

        public List<MakelaarCount> GetTop10(string searchString)
        {
            if (!_cache.TryGetValue(searchString, out List<MakelaarCount> result))
            {
                var url = _configuration["BaseUrl"] + _configuration["ApiKey"] + "/?type=koop&zo=" + searchString + "&page=1&pagesize=10000000";
                var root = CallApi(url);
                var objects = root["Objects"];

                var pageCount = Convert.ToInt32(root["Paging"]["AantalPaginas"]);
                var currentPage = Convert.ToInt32(root["Paging"]["HuidigePagina"]);

                if (pageCount > 1)
                    throw new Exception("Let op: het totaal aantal objecten past niet op 1 pagina");

                result = objects.GroupBy(o => o["MakelaarNaam"])
                               .Select(x => new MakelaarCount { MakelaarNaam = x.Key.ToString(), Count = x.Count() })
                               .OrderByDescending(x => x.Count)
                               .Take(10)
                               .ToList();

                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(cacheDuration));

                _cache.Set(searchString, result, cacheEntryOptions);
            }

            return result;
        }

        private JObject CallApi(string url)
        {
            using var httpClient = new HttpClient();
            using var response = httpClient.GetAsync(url).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var root = (JObject)JsonConvert.DeserializeObject(result);
                return root;
            }
            else
            {
                throw new Exception((int)response.StatusCode + "-" + response.ToString());
            }
        }

    }
}