using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AustroBohemia.Models
{
    public class BTCPrice
    {
        public DateTime Date { get; set; }
        public double Eur { get; set; }
        public double Czk { get; set; }

        public static async Task<double> RateEUR()
        {
            var url = "http://www.cnb.cz/cs/financni_trhy/devizovy_trh/kurzy_devizoveho_trhu/denni_kurz.txt?date=" + DateTime.Now.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
            using (var client = new HttpClient())
            {
                using (var response = await client.GetAsync(url))
                {
                    using (var content = response.Content)
                    {
                        var result = await content.ReadAsStringAsync();
                        var lines = result.Split('\n');
                        for (var i = 2; i < lines.Length; i++)
                        {
                            var value = lines[i].Trim().Split('|');
                            if (value.Length == 5 && value[3].ToString() == "EUR")
                            {
                                return Convert.ToDouble(value[4]);
                            }
                        }
                    }
                }
            }

            return 1;
        }
    }
}
