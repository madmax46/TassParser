using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TassParserLib;
using System.Threading;

namespace TassParserController
{
    public class Model
    {
        static int sleepIntervalInSecForUrls = 5 * 60 * 1000;
        static int sleepIntervalInSecForBody = 60 * 1000;

        CancellationTokenSource cancelTokenSourceCrawler = new CancellationTokenSource();
        Task crawlerTask;

        public void StartCrawler()
        {
            if (crawlerTask == null || crawlerTask?.IsCanceled == true || crawlerTask?.IsCompleted == true)
            {
                cancelTokenSourceCrawler = new CancellationTokenSource();
                crawlerTask = new Task(() => CrawlerMainAction(cancelTokenSourceCrawler), cancelTokenSourceCrawler.Token);
                crawlerTask.Start();
            }
        }

        public void StopCrawler()
        {
            cancelTokenSourceCrawler.Cancel();
        }
        public void StartParsingNews()
        {
            Task parserTask = Task.Factory.StartNew(ParserNewsMainAction);
        }

        private void CrawlerMainAction(CancellationTokenSource cancellationToken = null)
        {
            CrawlerUrlsOfNews urlOfNewsParser = new CrawlerUrlsOfNews(DBUtils.MyWrap);
            while (true)
            {
                try
                {
                    urlOfNewsParser.StartLoadNews(cancellationToken);
                    urlOfNewsParser.StartLoadFreshNews(cancellationToken);
                    Thread.Sleep(sleepIntervalInSecForUrls);
                }
                catch (OperationCanceledException OcEx)
                {
                    break;
                }
            }
        }

        private void ParserNewsMainAction()
        {
            NewsFromUrlParser newsFromUrlParser = new NewsFromUrlParser(DBUtils.MyWrap);
            while (true)
            {
                newsFromUrlParser.StartParsingNews();
                Thread.Sleep(sleepIntervalInSecForBody);
            }
        }
    }
}
