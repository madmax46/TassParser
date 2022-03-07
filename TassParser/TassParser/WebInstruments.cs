using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TassParserLib
{

    public class HttpRequestParameters
    {
        public string URL { get; set; }
        public string Reffer { get; set; }
        public string RequestUri { get; set; }

    }

    public class PostHttpRequestParametres : HttpRequestParameters
    {
        public string PostParameters { get; set; }

        public override string ToString()
        {
            return $"URL {URL}; Reffer {Reffer}; RequestUri {RequestUri}; PostParameters {PostParameters};";
        }
    }


    public class WebInstruments
    {

        public string GetDocument(string url)
        {
            StringBuilder resBuilder = new StringBuilder();
            string line;
            WebClient client = new WebClient();
            client.Encoding = Encoding.GetEncoding(1251);
            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            Stream data = client.OpenRead(url);
            StreamReader reader = new StreamReader(data, Encoding.GetEncoding(1251));
            while ((line = reader.ReadLine()) != null)
            {
                line = reader.ReadLine();
                resBuilder.AppendLine(line);
            }
            data.Close();
            reader.Close();
            return resBuilder.ToString();
        }

        public string GetDocumentAsHttpClient(string url, string reffer = "", string requestUri = "")
        {
            StringBuilder resBuilder = new StringBuilder();

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            if (!string.IsNullOrEmpty(reffer))
                client.DefaultRequestHeaders.Referrer = new Uri(reffer);
            HttpResponseMessage response = client.GetAsync(requestUri).Result;
            if (response.IsSuccessStatusCode)
            {
                var encoding = GetEncodingFromContentType(response);

                using (var res = response.Content.ReadAsStreamAsync().Result)
                {
                    resBuilder = ReadFromStream(res, encoding);
                }
            }

            return resBuilder.ToString();
        }

        public string GetDocumentAsHttpClient(HttpRequestParameters parametres)
        {
            StringBuilder resBuilder = new StringBuilder();

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(parametres.URL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36 Edge/17.17134");
            if (!string.IsNullOrEmpty(parametres.Reffer))
                client.DefaultRequestHeaders.Referrer = new Uri(parametres.Reffer);
            HttpResponseMessage response = client.GetAsync(parametres.RequestUri).Result;
            if (response.IsSuccessStatusCode)
            {
                var encoding = GetEncodingFromContentType(response);

                using (var res = response.Content.ReadAsStreamAsync().Result)
                {
                    resBuilder = ReadFromStream(res, encoding);
                }
            }

            return resBuilder.ToString();
        }

        private static StringBuilder ReadFromStream(Stream res, Encoding encoding)
        {
            StringBuilder resBuilder = new StringBuilder();
            string line;
            using (StreamReader reader = new StreamReader(res, encoding))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    resBuilder.AppendLine(line);
                }
            }

            return resBuilder;
        }

        public string PostDocumentAsHttpClient(PostHttpRequestParametres parametres)
        {
            StringBuilder resBuilder = new StringBuilder();
            HttpClient client = new HttpClient();

            var content = new StringContent(parametres.PostParameters, Encoding.UTF8, "application/json");

            client.BaseAddress = new Uri(parametres.URL);
            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36 Edge/17.17134");
            client.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));

            if (!string.IsNullOrEmpty(parametres.Reffer))
                client.DefaultRequestHeaders.Referrer = new Uri(parametres.Reffer);

            HttpResponseMessage response = client.PostAsync(parametres.RequestUri, content).Result;
            if (response.IsSuccessStatusCode == true)
            {
                var encoding = GetEncodingFromContentType(response);
                using (var res = response.Content.ReadAsStreamAsync().Result)
                {
                    resBuilder = ReadFromStream(res, encoding);
                }
            }

            return resBuilder.ToString();
        }

        public string PostDocumentAsHttpClientToTassApi(PostHttpRequestParametres parametres)
        {
            StringBuilder resBuilder = new StringBuilder();
            HttpClient client = new HttpClient();

            var content = new StringContent(parametres.PostParameters, Encoding.UTF8, "application/json");

            client.BaseAddress = new Uri(parametres.URL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36 Edge/17.17134");
            client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            client.DefaultRequestHeaders.Add("Accept-Language", "ru-RU");
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
            client.DefaultRequestHeaders.Add("Origin", "https://tass.ru");
            client.DefaultRequestHeaders.Add("Host", "tass.ru");
            client.DefaultRequestHeaders.Add("Connection", "Keep-Alive");

            if (!string.IsNullOrEmpty(parametres.Reffer))
                client.DefaultRequestHeaders.Referrer = new Uri(parametres.Reffer);

            HttpResponseMessage response = client.PostAsync(parametres.RequestUri, content).Result;
            if (response.IsSuccessStatusCode == true)
            {
                var encoding = GetEncodingFromContentType(response);
                using (var res = response.Content.ReadAsStreamAsync().Result)
                {
                    resBuilder = ReadFromStream(res, encoding);
                }
            }

            return resBuilder.ToString();
        }

        public Encoding GetEncodingFromContentType(HttpResponseMessage response)
        {
            var encodFromResponse = response.Content.Headers.ContentType.CharSet;

            switch (encodFromResponse)
            {
                case "utf-8": return Encoding.UTF8;
                case "utf-32": return Encoding.UTF32;
                case "utf-7": return Encoding.UTF7;
                case "windows-1251": return Encoding.GetEncoding("1251");
                default: return Encoding.UTF8;
            }
        }


    }
}
