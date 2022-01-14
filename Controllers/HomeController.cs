using AustroBohemia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AustroBohemia.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            List<BTCPrice> prices = new List<BTCPrice>();
            return View(prices);
        }

        [HttpPost]
        public async Task<JsonResult> GetBTCPrice()
        {
            List<BTCPrice> prices = new List<BTCPrice>();
            var url = "https://api.coindesk.com/v1/bpi/currentprice.json";
            using (var client = new HttpClient())
            {
                using (var response = await client.GetAsync(url))
                {
                    using (var content = response.Content)
                    {
                        var model = new BTCPrice();
                        var result = await content.ReadAsStringAsync();
                        var root = (JObject)JsonConvert.DeserializeObject(result);

                        model.Date = DateTime.Now;

                        var bpiToken = root.SelectToken("bpi").OfType<JProperty>().ToDictionary(p => p.Name, p => p.Value);
                        if (bpiToken.ContainsKey("EUR"))
                        {
                            var eurToken = bpiToken["EUR"].SelectToken("").OfType<JProperty>().ToDictionary(p => p.Name, p => p.Value);
                            if (eurToken.ContainsKey("rate_float"))
                            {
                                var rate = Convert.ToDouble(eurToken["rate_float"]);
                                model.Eur = Math.Round(rate, 2);
                            }
                        }

                        var rateEur = await BTCPrice.RateEUR();
                        if (rateEur > 1 && model.Eur > 0)
                        {
                            model.Czk = Math.Round(rateEur * model.Eur, 2);
                        }

                        prices.Add(model);
                    }
                }
            }

            return Json(prices);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
