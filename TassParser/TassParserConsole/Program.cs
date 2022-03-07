using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using TassParserLib;

namespace TassParserConsole
{
    class Program
    {
        static int sleepIntervalInSecForUrls = 60 * 1000;

        static void Main(string[] args)
        {
            InitNlog();
            //StartCrawler();
            StartParsingNews();
            Console.Read();
        }

        private static void StartCrawler()
        {
            Task crawlerTask = Task.Factory.StartNew(CrawlerMainAction);
        }



        private static void StartParsingNews()
        {
            Task parserTask = Task.Factory.StartNew(ParserNewsMainAction);
        }



        private static void CrawlerMainAction()
        {
            CrawlerUrlsOfNews urlOfNewsParser = new CrawlerUrlsOfNews(DBUtils.MyWrapUniver);
            while (true)
            {
                urlOfNewsParser.StartLoadNews();
                urlOfNewsParser.StartLoadFreshNews();
                Thread.Sleep(sleepIntervalInSecForUrls);
            }
        }

        private static void ParserNewsMainAction()
        {
            NewsFromUrlParser newsFromUrlParser = new NewsFromUrlParser(DBUtils.MyWrapUniver, @"D:\");
            while (true)
            {
                newsFromUrlParser.StartParsingNews();
                Thread.Sleep(sleepIntervalInSecForUrls);
            }
        }

        private static void InitNlog()
        {
            var config = new NLog.Config.LoggingConfiguration();

            //var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "log.txt" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            //var logfile = new NLog.Targets.FileTarget("AsyncWrapper") { FileName = "log.txt" };

            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            //config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);

            LogManager.Configuration = config;
        }

    }
}
