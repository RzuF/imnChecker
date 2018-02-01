using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace imnChecker
{
    public class ConnetionProvider
    {
        private const string GradeUrl = "https://dziekanat.agh.edu.pl/OcenyP.aspx";
        private const string LoginUrl = "https://dziekanat.agh.edu.pl/Logowanie2.aspx?ReturnUrl=%2fOcenyP.aspx";
        private const string NotLoggedSearchPhrase = "<span id=\"ctl00_ctl00_ContentPlaceHolder_MiddleContentPlaceHolder_Label3\" class=\"label\">Has³o:</span>";

        private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.84 Safari/537.36";
        private const string Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
        private const string Referer = "https://dziekanat.agh.edu.pl/Logowanie2.aspx?ReturnUrl=%2fOcenyP.aspx";
        private const string ContentType = "application/x-www-form-urlencoded";

        public Regex GradeRegEx { get; set; } = new Regex(@"(\d*\.\d{1}){1}.*");
        public string Item { get; set; }
        public string Type { get; set; } = "Egzamin";

        public ConnetionProvider()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            _httpClient.DefaultRequestHeaders.Add("Accept", Accept);
            _httpClient.DefaultRequestHeaders.Add("Referer", Referer);           
        }

        public string IndexNumber { get; set; }
        public string Pass { get; set; }

        private readonly HttpClient _httpClient = new HttpClient(new WebRequestHandler
        {
            ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true
        });

        public async Task<bool> CheckGrades()
        {
            var result = await _httpClient.GetStringAsync(GradeUrl);

            result = await LoginIfNot(result);

            var htmlDocument = GetHtmlDocumentFromHtmlString(result);
            var isGradePresent = IsGradePresentInHtmlDocument(htmlDocument, Item, Type);

            return isGradePresent;
        }

        private async Task<string> LoginIfNot(string result)
        {
            if (!result.Contains(NotLoggedSearchPhrase))
            {
                return result;
            }
                        
            string viewState = result.FindBetween("<input type=\"hidden\" name=\"__VIEWSTATE\" id=\"__VIEWSTATE\" value=\"", "\"");

            string postString = "ctl00_ctl00_ScriptManager1_HiddenField=&__EVENTTARGET=&__EVENTARGUMENT=&__VIEWSTATE="
                                + Uri.EscapeDataString(viewState)
                                + "&__VIEWSTATEGENERATOR=BBDE9B47&ctl00_ctl00_TopMenuPlaceHolder_TopMenuContentPlaceHolder_MenuTop3_menuTop3_ClientState=&ctl00%24ctl00%24ContentPlaceHolder%24MiddleContentPlaceHolder%24txtIdent="
                                + IndexNumber
                                + "&ctl00%24ctl00%24ContentPlaceHolder%24MiddleContentPlaceHolder%24txtHaslo="
                                + Uri.EscapeDataString(Pass)
                                + "&ctl00%24ctl00%24ContentPlaceHolder%24MiddleContentPlaceHolder%24butLoguj=Zaloguj&ctl00%24ctl00%24ContentPlaceHolder%24MiddleContentPlaceHolder%24rbKto=student";
                                                      
            var loginResult = await _httpClient.PostAsync(LoginUrl, new StringContent(postString, Encoding.UTF8, ContentType));

            return await loginResult.Content.ReadAsStringAsync();
        }        

        HtmlDocument GetHtmlDocumentFromHtmlString(string html)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            return htmlDocument;
        }

        bool IsGradePresentInHtmlDocument(HtmlDocument htmlDocument, string item, string type = "Egzamin")
        {
            var table = htmlDocument.GetElementbyId("ctl00_ctl00_ContentPlaceHolder_RightContentPlaceHolder_dgDane");
            var tds = table.Descendants("tr").FirstOrDefault(x => x.Descendants("td").Any(y => y.InnerText == item) && x.Descendants("td").Any(y => y.InnerText == type));

            var matches = tds?.Descendants("td").Select(x => GradeRegEx.Match(x.InnerText));
            var gradePresent = tds?.Descendants("td").Any(x => GradeRegEx.IsMatch(x.InnerText));

            return gradePresent ?? false;
        }
    }
}