using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TassParserLib;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using System.Linq;
using NLog;

namespace TassTests
{
    [TestClass]
    public class UnitTest1
    {
        private static void InitNlog()
        {
            var config = new NLog.Config.LoggingConfiguration();

            //var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "log.txt" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            var logfile = new NLog.Targets.FileTarget("AsyncWrapper") { FileName = "log.txt" };

            //config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);

            LogManager.Configuration = config;
        }

        [TestMethod]
        public void TestMethod1()
        {


            //var res1 = MySQLWrap.ToMySqlParamStat("Moody's: ипотечные каникулы негативно повлияют на кредитное качество ипотечных бумаг");
            //var str = Path.Combine("https://tass.ru", "/ekonomika");
            InitNlog();





            TassParserLib.CrawlerUrlsOfNews parser = new TassParserLib.CrawlerUrlsOfNews(TassParserLib.DBUtils.MyWrap);
            parser.LoadHeadingsFromDb();
            parser.StartLoadNewsByHeading(parser.TassHeadings[3]);





            long endDate = Utils.DateTimeToUnix(new DateTime(2018, 12, 09, 12, 10, 11));
            long endDate2 = Utils.DateTimeToUnix(new DateTime(2018, 12, 09, 12, 10, 10));


            //  JsonValue value = JsonValue.Parse(@"{ ""name"":""Prince Charming"", ...");

            string json = "";

            json = File.ReadAllText(@"C:\Users\madmax\Desktop\пример возвращаемых данных.txt", Encoding.UTF8);
            CategoryNewsListResponse sd = new CategoryNewsListResponse();
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var serializer = new DataContractJsonSerializer(typeof(CategoryNewsListResponse));
                sd = serializer.ReadObject(stream) as CategoryNewsListResponse;
            }



            var query = DBUtils.CategoryNewsToInsertSql(sd.NewsList);


            JSONParamsLoadNews postJSONParameters = new JSONParamsLoadNews()
            {
                imageSize = 434,
                Limit = 20,
                SectionId = 25,
                Timestamp = 1544197256,
                Type = "all"
            };

            var parameters = postJSONParameters.ToString();

            TassParserLib.PostHttpRequestParametres parametres = new TassParserLib.PostHttpRequestParametres()
            {
                URL = "https://tass.ru",
                Reffer = "https://tass.ru/ekonomika",
                RequestUri = "/userApi/categoryNewsList",
                PostParameters = parameters
            };


            TassParserLib.WebInstruments webInstruments = new TassParserLib.WebInstruments();
            var da = webInstruments.PostDocumentAsHttpClientToTassApi(parametres);
            //var content = webInstruments.GetDocumentAsHttpClient("http://tass.ru", reffer: "http://tass.ru", requestUri: "/v-strane");
            //TassParser.UrlOfNewsParser parser = new TassParser.UrlOfNewsParser(TassParser.DBUtils.MyWrap);
            //parser.LoadAndSaveHeadingsToDb();
            //parser.LoadHeadingsFromDb();

            //var val = parser.ParseSectionIdFromPage(content);
        }





        [TestMethod]
        public void TestParsePage()
        {



            InitNlog();





            NewsFromUrlParser newsFromUrlParser = new NewsFromUrlParser(DBUtils.MyWrap, @"G:\ParsedNews\TassNews");
            newsFromUrlParser.ParseNewsFromUrlTest(5548459, "/politika/5548459");
            //newsFromUrlParser.ParseNewsFromUrlTest(5915986, "/ekonomika/5915986");
            
            //newsFromUrlParser.StartParsingNews();
        }



        private void InitFromDumpDB()
        {
            var text = File.ReadAllText(@"E:\dump2\Новый текстовый документ.txt");

            var lines = text.Split('|');
            foreach (var oneLine in lines)
            {
                DBUtils.MyWrapUniver.Execute(oneLine);
            }
        }

    }
}
