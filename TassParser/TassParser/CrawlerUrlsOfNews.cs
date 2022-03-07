using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;
using System.Data;
using System.IO;
using NLog;
using System.Threading;
using System.Diagnostics;

namespace TassParserLib
{
    public class CrawlerUrlsOfNews
    {
        private Logger logger = LogManager.GetCurrentClassLogger();

        public const string MainPage = "http://tass.ru/";
        public const string MainPageHttps = "https://tass.ru/";
        public const string UrlToApi = "https://tass.ru/userApi/categoryNewsList";
        public DateTime EndDateTime = new DateTime(2016, 01, 01);
        public string HeadingsFindRegExPattern = "<a\\s*class=\"menu-sections-list__title\"\\s*href\\s*=\\s*[\"*,\'*](.+?)[\"*,\'*].+?>(.+?)</a>";
        public string SectionIdFindRegExPattern = "sectionId\\s*=\\s*(\\d*?);";

        IDBProvider dBProvider { get; }
        WebInstruments webInstruments { get; set; }
        CancellationTokenSource m_cancellationTokenSource { get; set; }
        public List<TassHeading> TassHeadings { get; set; }

        public CrawlerUrlsOfNews(IDBProvider idBProvider)
        {
            webInstruments = new WebInstruments();
            dBProvider = idBProvider;
            TassHeadings = new List<TassHeading>();
        }

        public void LoadHeadingsFromSite()
        {
            TassHeadings = LoadHeadingsFromMainSite().ToList();

        }

        private IEnumerable<TassHeading> LoadHeadingsFromMainSite()
        {
            var mainSite = webInstruments.GetDocumentAsHttpClient(MainPage);
            var headings = ParseHeadingsFromMainPage(mainSite);

            headings = FillSectionId(headings);
            return headings;
        }

        public void LoadAndSaveHeadingsToDb()
        {
            var headings = LoadHeadingsFromMainSite();
            SaveHeadingsToDb(headings);
        }

        public IEnumerable<TassHeading> ParseHeadingsFromMainPage(string pageBody)
        {
            var matches = Regex.Matches(pageBody, HeadingsFindRegExPattern);

            List<TassHeading> urlToHeadings = new List<TassHeading>();
            foreach (Match oneMatch in matches)
            {
                urlToHeadings.Add(new TassHeading(oneMatch.Groups[1].Value, oneMatch.Groups[2].Value));
            }

            return urlToHeadings;
        }

        private int ParseSectionIdFromPage(string pageBody)
        {
            var matches = Regex.Matches(pageBody, SectionIdFindRegExPattern);
            if (matches.Count == 0)
                return -1;

            int retVal = -1;
            if (int.TryParse(matches[0].Groups[1]?.Value, out retVal))
                return retVal;

            return -1;
        }

        private IEnumerable<TassHeading> FillSectionId(IEnumerable<TassHeading> source)
        {
            foreach (var oneHeading in source)
            {
                var body = webInstruments.GetDocumentAsHttpClient(MainPage, MainPage, oneHeading.URLPart);
                var sectionId = ParseSectionIdFromPage(body);
                if (sectionId == -1)
                    continue;

                oneHeading.SectionId = sectionId;
            }

            return source;
        }


        private void SaveHeadingsToDb(IEnumerable<TassHeading> source)
        {
            if (source?.Any() != true)
                return;

            string query = DBUtils.HeadigsToInsertSql(source);

            try
            {
                dBProvider.Execute(query);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void LoadHeadingsFromDb()
        {
            var headings = GetHeadingsFromDb();
            TassHeadings = headings.ToList();
        }

        private IEnumerable<TassHeading> GetHeadingsFromDb()
        {
            string query = "SELECT sectionId, name, url, isAllowParse FROM tass_headings th;";
            List<TassHeading> headings = new List<TassHeading>();

            try
            {
                var dt = dBProvider.GetDataTable(query);

                foreach (DataRow oneRow in dt.Rows)
                {
                    headings.Add(TassHeading.FromRow(oneRow));
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return headings;
        }

        public void StartLoadNews(CancellationTokenSource cancellationToken = null)
        {
            if (cancellationToken != null)
                m_cancellationTokenSource = cancellationToken;

            logger.Info("стартую crawler");
            MainLoadNews();
            logger.Info("crawler закончил свою работу");
        }

        private void MainLoadNews(bool isLoadFresh = false)
        {
            CheckOnExistHeadings();

            if (TassHeadings?.Any() != true)
                return;
            string isFreshTextVal = isLoadFresh ? "свежие " : "";
            foreach (var oneHeading in TassHeadings.Where(r => r.IsAllowParse))
            {
                logger.Info($"Начал парсить {isFreshTextVal}новости для {oneHeading}");
                try
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    StartLoadNewsByHeading(oneHeading, isLoadFresh);
                    logger.Info($"Закончил парсить {isFreshTextVal}новости для {oneHeading}  За время {stopwatch.ElapsedMilliseconds} ms");
                }
                catch (OperationCanceledException OcEx)
                {
                    logger.Warn("crawler отменен");
                    m_cancellationTokenSource.Token.ThrowIfCancellationRequested();
                }
                catch (Exception ex)
                {
                    logger.Error($"Ошибка в загрузке {oneHeading} \n {ex}");
                }
            }
        }

        private void CheckOnExistHeadings()
        {
            if (TassHeadings == null || !TassHeadings.Any())
            {
                LoadHeadingsFromDb();
                if (TassHeadings?.Any() != true)
                {
                    LoadHeadingsFromSite();
                    SaveHeadingsToDb(TassHeadings);
                }
            }
        }

        public void StartLoadFreshNews(CancellationTokenSource cancellationToken = null)
        {
            if (cancellationToken != null)
                m_cancellationTokenSource = cancellationToken;

            logger.Info("стартую crawler для новых новостей");
            MainLoadNews(true);
            logger.Info("crawler для новых новостей закончил свою работу");

        }


        public void StartLoadNewsByHeading(TassHeading heading, bool isLoadFreshNews = false)
        {
            var dates = LoadMaxAndMinParseDt(heading);
            long lastDate = dates.Item1;
            long endDate = Utils.DateTimeToUnix(EndDateTime);

            if (dates.Item1 == dates.Item2) //если вдруг была ошибка доступа к базе, то вообще ничего не парсим
                endDate = dates.Item2;

            if (isLoadFreshNews)
            {
                lastDate = Utils.DateTimeToUnix(DateTime.Now);
                endDate = dates.Item2;
            }

            MainParseUrlsByHeadings(heading, lastDate, endDate);
        }

        private void MainParseUrlsByHeadings(TassHeading heading, long lastDate, long endDate)
        {

            JSONParamsLoadNews postJSONParameters = new JSONParamsLoadNews()
            {
                imageSize = 434,
                Limit = 20,
                SectionId = heading.SectionId,
                Timestamp = lastDate,
                Type = "all"
            };

            PostHttpRequestParametres requestPost = new PostHttpRequestParametres()
            {
                URL = "https://tass.ru",
                Reffer = string.Concat("https://tass.ru", heading.URLPart),
                RequestUri = "/userApi/categoryNewsList",
                PostParameters = postJSONParameters.ToString()
            };

            CategoryNewsListResponse categoryNewsList = new CategoryNewsListResponse();

            while (lastDate > endDate)
            {
                if (m_cancellationTokenSource?.Token != null)
                {
                    if (m_cancellationTokenSource.Token.IsCancellationRequested)
                        m_cancellationTokenSource.Token.ThrowIfCancellationRequested();
                }
                postJSONParameters.Timestamp = lastDate;
                requestPost.PostParameters = postJSONParameters.ToString();

                var postResult = webInstruments.PostDocumentAsHttpClientToTassApi(requestPost);

                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(postResult)))
                {
                    var serializer = new DataContractJsonSerializer(typeof(CategoryNewsListResponse));
                    categoryNewsList = serializer.ReadObject(stream) as CategoryNewsListResponse;
                }

                logger.Info($"SectionId = {heading.SectionId} загружено 20 новостей начиная с date {lastDate}  {Utils.UnixTimeStampToDateTime(lastDate)}");

                if (categoryNewsList == null)
                {
                    logger.Warn($"categoryNewsList = Null  {requestPost.ToString()} {Environment.NewLine} Результат запроса {postResult}");
                    return;
                }

                var query = DBUtils.CategoryNewsToInsertSql(categoryNewsList.NewsList);
                var queryInsertHeadingsAndNews = DBUtils.CategoryNewsToHeadingsAndNewsInsert(categoryNewsList.NewsList, heading.SectionId);
                try
                {
                    var countRows = dBProvider.Execute(string.Concat(query, queryInsertHeadingsAndNews));
                }
                catch (Exception ex)
                {
                    logger.Error($"{requestPost.ToString()} {Environment.NewLine}Результат запроса {postResult}{Environment.NewLine}{ex}");
                }

                lastDate = categoryNewsList.NewsList.Min(r => r.Date);
                //if (lastDate == categoryNewsList.LastTime)
                //{
                //    logger.Debug($"{lastDate} == {categoryNewsList.LastTime}");
                //}
                //SaveLastParsedDt(heading, lastDate);
                System.Threading.Thread.Sleep(1000);
            }
        }

        private bool SaveLastParsedDt(TassHeading heading, long lastDt)
        {
            string query = $"INSERT INTO headings_parse_constants (sectionId, lastParsedDt) VALUES ({MySQLWrap.ToMySqlParamStat(heading.SectionId)}, {MySQLWrap.ToMySqlParamStat(lastDt)}) ON DUPLICATE KEY UPDATE lastParsedDt = VALUES(`lastParsedDt`);";

            try
            {
                dBProvider.Execute(query);
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return false;
            }
        }

        private long LoadLadsParsedDt(TassHeading heading)
        {
            long lastDate = Utils.DateTimeToUnix(DateTime.Now);

            string query = $"SELECT hpc.lastParsedDt FROM headings_parse_constants hpc WHERE hpc.sectionId = {heading.SectionId};";
            var dt = dBProvider.GetDataTable(query);

            if (dt.Rows.Count == 0)
                return lastDate;

            long res = Convert.IsDBNull(dt.Rows[0][0]) ? lastDate : Convert.ToInt64(dt.Rows[0][0]);

            return res;
        }

        private Tuple<long, long> LoadMaxAndMinParseDt(TassHeading heading)
        {

            long min = Utils.DateTimeToUnix(DateTime.Now);
            long max = Utils.DateTimeToUnix(DateTime.Now);

            DataTable dt = new DataTable();
            //string query = $"SELECT hpc.lastParsedDt FROM headings_parse_constants hpc WHERE hpc.sectionId = {heading.SectionId};";
            try
            {
                dt = dBProvider.ProcedureByName("GetMinAndMaxDtParseUrls", heading.SectionId);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            if (dt.Rows.Count == 0)
                return new Tuple<long, long>(min, max);
            min = Convert.IsDBNull(dt.Rows[0]["min"]) ? min : Convert.ToInt64(dt.Rows[0]["min"]);
            max = Convert.IsDBNull(dt.Rows[0]["max"]) ? max : Convert.ToInt64(dt.Rows[0]["max"]);

            return new Tuple<long, long>(min, max);

        }

    }
}
