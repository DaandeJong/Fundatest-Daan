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

        private const double _cacheDuration = 10;
        private const string _koopOfHuur = "koop";

        private IMemoryCache _cache;
        private readonly IConfiguration _configuration;

        public FundaService(IMemoryCache memoryCache, IConfiguration configuration)
        {
            _cache = memoryCache;
            _configuration = configuration;
        }

        public List<MakelaarCount> GetMakelaarTop10(string searchString)
        {
            var jsonResult = GetSearchResult(_koopOfHuur, searchString);
            return GetMakelaarTop10FromJson(jsonResult);
        }

        /// <summary>
        /// Deze methode kan (automatisch) getest worden met een json tekstbestand
        /// </summary>
        /// <param name="json"></param>
        /// <returns>List<MakelaarCount></returns>
        private List<MakelaarCount> GetMakelaarTop10FromJson(string json)
        {          
            return GetObjectsFromJson(json)
                           .GroupBy(o => o["MakelaarNaam"])
                           .Select(x => new MakelaarCount
                           {
                               MakelaarNaam = x.Key.ToString(),
                               Count = x.Count()
                           })
                           .OrderByDescending(x => x.Count)
                           .Take(10)
                           .ToList();
        }
                
        private JToken GetObjectsFromJson(string json)
        {
            var root = (JObject)JsonConvert.DeserializeObject(json);
            var objects = root["Objects"];

            if (Convert.ToInt32(root["Paging"]["AantalPaginas"]) > 1)
                throw new Exception("Let op: het totaal aantal objecten past niet op 1 pagina");

            return objects;
        }

        // Elke search wordt gecached zodat geen onnodige aanroepen van de api en snelheidswinst
        private string GetSearchResult(string type, string searchString)
        {
            if (!_cache.TryGetValue(type + searchString, out string result))
            {
                var url = $"{_configuration["BaseUrl"]}{ _configuration["ApiKey"]}/?type={type}&zo={searchString}&page=1&pagesize=10000000";
                result = CallApi(url);

                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(_cacheDuration));
                _cache.Set(type + searchString, result, cacheEntryOptions);
            }
            return result;
        }

        // Ik heb gekozen voor de REST /json aanpak ipv WCF omdat
        // - de wcf endpoints niet werkten
        // - ik het leuker vond json te gebruiken (dan een voorgegenereerde wcf proxy)
        // - het voor deze specifieke opdracht alles lichtgewicht houdt
        // Nadelen oa:
        // - meer werk om uit te breiden oa door geen gegenereerde classes 
        // - niet strongly typed
        // - bij update van de api moet je meer handmatig aanpassen
        private string CallApi(string url)
        {
            using var httpClient = new HttpClient();
            using var response = httpClient.GetAsync(url).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            else          
                throw new Exception((int)response.StatusCode + "-" + response.ToString());            
        }

    }
}