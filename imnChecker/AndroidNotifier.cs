using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;

namespace imnChecker
{
    public class AndroidNotifier : INotifier
    {
        private readonly HttpClient _httpClient = new HttpClient();
        public string ApiKey { get; set; }
        public async Task<bool> Authenteticate(string additionalJsonArgs = "")
        {
            var result =
                await _httpClient.GetStringAsync($"https://www.notifymyandroid.com/publicapi/verify?apikey={ApiKey}");

            return false;
        }

        public async Task<string> SendNotification(string message, string additionalJsonArgs = "")
        {
            var postDataDictionary = new Dictionary<string, string>
            {
                {"apikey", ApiKey},
                {"application", "GradeNotifier" },
                {"event", "NewGrade" },
                {"description", message }
            };

            var postDataString = string.Join("&", postDataDictionary.Select(x => x.Key + "=" + x.Value));

            var result = await _httpClient.GetStringAsync($"https://www.notifymyandroid.com/publicapi/notify?{postDataString}");

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(result);
            string jsonText = JsonConvert.SerializeXmlNode(doc);

            var json = JsonConvert.DeserializeObject<dynamic>(jsonText);

            try
            {
                var status = json["nma"]["success"]["@code"].Value;
                return status;
            }
            catch (Exception)
            {
                var status = json["nma"]["error"]["@code"].Value;
                return status;
            }
        }
    }
}
