using Fundatest.Model;
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
        public string _baseUrl = "http://partnerapi.funda.nl/feeds/Aanbod.svc/json/";
        public string _key = "ac1b0b1572524640a0ecc54de453ea9f";

        public List<MakelaarCount> GetTop10(string searchString)
        {
            var url = _baseUrl + _key + "/?type=koop&zo=" + searchString + "&page=1&pagesize=10000000";                   
            var root = CallApi(url);
            var objects = root["Objects"];

            var pageCount = Convert.ToInt32(root["Paging"]["AantalPaginas"]);
            var currentPage = Convert.ToInt32(root["Paging"]["HuidigePagina"]);

            if (pageCount > 1)
                throw new Exception("Let op: het totaal aantal objecten past niet op 1 pagina");

            return objects.GroupBy(o => o["MakelaarNaam"])
                           .Select(x => new MakelaarCount { MakelaarNaam = x.Key.ToString(), Count = x.Count() })
                           .OrderByDescending(x => x.Count)
                           .Take(10)
                           .ToList();
        }

        private JObject CallApi(string url)
        {
            using (var httpClient = new HttpClient())
            {
                using (var response = httpClient.GetAsync(url).GetAwaiter().GetResult())
                {
                    using (var content = response.Content)
                    {
                        var result = content.ReadAsStringAsync().GetAwaiter().GetResult();
                        var root = (JObject)JsonConvert.DeserializeObject(result);
                        return root;
                    }
                }
            }
        }

    }
}