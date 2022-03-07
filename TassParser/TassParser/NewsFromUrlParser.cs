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
using System.Xml.Serialization;
using System.Threading;

namespace TassParserLib
{
    public class NewsFromUrlParser
    {
        private Logger logger = LogManager.GetCurrentClassLogger();

        IDBProvider dbProvider { get; }
        WebInstruments webInstruments { get; set; }
        Queue<CategoryNews> сategoryNewsParseQueue { get; set; }
        HttpRequestParameters httpRequesParamsToLoadNews { get; set; }
        string pathToSaveFiles { get; set; }
        string folderName = "ParsedNews";
        public string tagsRegExPattern = "<a\\s*class=\"tags__item\"\\s*href\\s*=\\s*[\"*,\'*](.+?)[\"*,\'*].+?>(.+?)</a>";
        public string tagsRegExPattern2 = "<a.*?class=\"tags__item\".*?>(.+?)<\\/a>";
        public string hrefRegExPattern = "href\\s*=\\s*[\"*,\'*](.+?)[\"*,\'*]";

        public string mainTextFromPageRegExPattern = "<div class=\"text-block\">(.+?)<\\/div>";
        public string mainTextFromPageRegExPatternSecond = "<div class=\"article__text\">(.+?)<\\/div>";


        public NewsFromUrlParser(IDBProvider dBProvider)
        {
            this.dbProvider = dBProvider;
            webInstruments = new WebInstruments();
            сategoryNewsParseQueue = new Queue<CategoryNews>();
            httpRequesParamsToLoadNews = new HttpRequestParameters()
            {
                URL = "https://tass.ru",
                Reffer = "https://google.ru"
            };
        }

        public NewsFromUrlParser(IDBProvider dBProvider, string m_pathToFiles) : this(dBProvider)
        {
            pathToSaveFiles = Path.Combine(m_pathToFiles, folderName);
        }

        public void StartParsingNews()
        {
            MainParsingNews();
        }

        private void MainParsingNews()
        {
            logger.Info("Начал парсить текст новостей");

            var newsToParse = GetNextCategoryNewsToParse();
            while (newsToParse != null)
            {
                ParseAndSaveNewsByCategoryNews(newsToParse);

                newsToParse = GetNextCategoryNewsToParse();
                Thread.Sleep(1000);
            }
            logger.Info("Закончил парсить текст новостей");
        }

        private void ParseAndSaveNewsByCategoryNews(CategoryNews newsToParse)
        {
            var parsedNews = ParseNewsBodyByUrl(newsToParse.Link);

            if (parsedNews != null)
            {
                //var saveResult = SaveParsedNewsToFile(newsToParse, parsedNews);
                var saveResult = SaveParsedNewsToDB(newsToParse, parsedNews);

                if (saveResult)
                {
                    newsToParse.TimeParseNews = DateTime.Now;
                    logger.Info(newsToParse);
                }
                else
                {
                    logger.Warn($"Не сохранилась новость  {newsToParse} {parsedNews}");
                }
            }
            else
            {
                newsToParse.ParseStatus = ParseStatus.Error;
            }

            SaveProgressInDB(newsToParse);
        }

        /// <summary>
        /// Метод только парсит новость по ссылке
        /// </summary>
        /// <param name="categoryNewsInfo"></param>
        /// <returns></returns>
        public ParsedFullNewsBody ParseNewsBodyByUrl(string link)
        {
            if (link.IndexOf('/') != 0)
                return null;

            httpRequesParamsToLoadNews.RequestUri = link;

            string page = TryLoadPage(httpRequesParamsToLoadNews);
            if (string.IsNullOrEmpty(page))
            {
                logger.Warn($"Пустая страница!!!, невозможно отпарсить \n {link}\n {page}");
                return null;
            }

            try
            {
                ParsedFullNewsBody parsedNews = ParseNewsBodyFromTextPage(page);
                if (parsedNews != null)
                {
                    return parsedNews;
                }
                else
                {
                    logger.Warn($"Пустое тело новости, невозможно отпарсить \n {link}\n {page}");
                    return null;
                }

            }
            catch (Exception ex)
            {
                logger.Error($"{ex} {Environment.NewLine}{page}");
                return null;
            }
        }

        private string TryLoadPage(HttpRequestParameters m_httpRequesParamsToLoadNews)
        {
            int iter = 0;
            while (iter++ < 5)
            {
                try
                {
                    return webInstruments.GetDocumentAsHttpClient(m_httpRequesParamsToLoadNews);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    Thread.Sleep(5000);
                }
            }
            return string.Empty;
        }

        private ParsedFullNewsBody ParseNewsBodyFromTextPage(string page)
        {
            ParsedFullNewsBody parsedNews = new ParsedFullNewsBody();
            var newsText = ParseMainTextFromTextPage(page);
            if (string.IsNullOrEmpty(newsText))
            {
                return null;
            }

            parsedNews.TextBlock = Utils.ClearTextFromHtmlTags(newsText);

            var tags = ParseTagsFromTextPage(page);
            if (tags.Any())
            {
                parsedNews.Tags = tags.ToList();
            }
            else
            {
                logger.Warn($"Нету тегов ");
            }
            parsedNews.DtLoad = DateTime.Now;
            return parsedNews;
        }

        private string ParseMainTextFromTextPage(string page)
        {
            Regex regexNewsBody = new Regex(mainTextFromPageRegExPattern, RegexOptions.Singleline);
            var matches = regexNewsBody.Matches(page);
            if (matches.Count == 0)
            {
                Regex regexNewsBodySecond = new Regex(mainTextFromPageRegExPatternSecond, RegexOptions.Singleline);
                matches = regexNewsBodySecond.Matches(page);
            }

            if (matches.Count > 0)
            {
                return FillTextBodyFromMatches(matches);
            }

            return string.Empty;
        }

        private static string FillTextBodyFromMatches(MatchCollection matches)
        {
            StringBuilder textBody = new StringBuilder();
            foreach (Match oneMatch in matches)
            {
                textBody.Append(oneMatch.Groups[1].Value);
            }
            return textBody.ToString();
        }

        private IEnumerable<NewsTag> ParseTagsFromTextPage(string page)
        {
            List<NewsTag> tags = new List<NewsTag>();
            //Regex regexNewsTags = new Regex("<a\\s*class=\"tags__item\"\\s*href\\s*=\\s*[\"*,\'*](.+?)[\"*,\'*].+?>(.+?)</a>");
            //var tagsMatches = Regex.Matches(page, tagsRegExPattern);
            var tagsMatches = Regex.Matches(page, tagsRegExPattern2);
            foreach (Match oneMatch in tagsMatches)
            {
                var hrefMatch = Regex.Match(oneMatch.Value, hrefRegExPattern);

                tags.Add(new NewsTag(hrefMatch.Groups[1].Value, Utils.ClearTextFromHtmlTags(oneMatch.Groups[1].Value)));

                //tags.Add(new NewsTag(oneMatch.Groups[1].Value, Utils.ClearTextFromHtmlTags(oneMatch.Groups[2].Value)));
            }
            return tags;
        }

        /// <summary>
        /// по сути контроллер,который подает следующую ссылку на скачивание
        /// </summary>
        /// <returns></returns>
        private CategoryNews GetNextCategoryNewsToParse()
        {

            if (!сategoryNewsParseQueue.Any())
            {
                var urlsFromDb = LoadUrlNewsFromDb();

                foreach (var oneCategoryNews in urlsFromDb)
                {
                    if (!IsAllowToParse(oneCategoryNews))
                    {
                        oneCategoryNews.ParseStatus = ParseStatus.NotAllow;
                        ChangeParseStatus(oneCategoryNews);
                    }
                    else
                    {
                        if (!сategoryNewsParseQueue.Any(r => r.Id == oneCategoryNews.Id))
                            сategoryNewsParseQueue.Enqueue(oneCategoryNews);
                    }
                }
            }

            if (сategoryNewsParseQueue.Any())
                return сategoryNewsParseQueue.Dequeue();

            return null;
        }

        private bool IsAllowToParse(CategoryNews categoryNews)
        {
            if (categoryNews.Link.IndexOf('/') != 0)
                return false;

            if (categoryNews.Link.IndexOf("/tests") == 0)
                return false;

            if (categoryNews.Link.IndexOf("/polls") == 0)
                return false;

            if (categoryNews.Link.IndexOf("/infographics") == 0)
                return false;
            return true;
        }


        #region работа с базой
        private void ChangeParseStatus(CategoryNews categoryNews)
        {
            string updateQuery = $"UPDATE tass_news SET parseStatus = {MySQLWrap.ToMySqlParamStat((int)categoryNews.ParseStatus)} WHERE id = {MySQLWrap.ToMySqlParamStat(categoryNews.Id)};";
            ExecuteQueryWithLogEx(updateQuery);
        }

        private IEnumerable<CategoryNews> LoadUrlNewsFromDb()
        {
            string query = "SELECT tn.id, tn.date, tn.title, tn.subtitle, tn.link, tn.parseStatus FROM tass_news tn FORCE INDEX(IDX_tass_news_parseStatus) WHERE tn.parseStatus = 0 limit 100";
            DataTable notParsedNewsDT = new DataTable();
            try
            {
                notParsedNewsDT = dbProvider.GetDataTable(query);
            }
            catch (Exception ex)
            {
                logger.Error($"ошибка получения новостей для парсинга из БД \n{ex}");
            }

            if (notParsedNewsDT.Rows.Count == 0)
            {
                logger.Warn("нету новостей для парсинга");
                return new List<CategoryNews>();
            }
            List<CategoryNews> notParsedNews = new List<CategoryNews>();
            foreach (DataRow oneRow in notParsedNewsDT.Rows)
            {
                try
                {
                    notParsedNews.Add(CategoryNews.FromRow(oneRow));
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
            return notParsedNews;
        }

        private void ExecuteQueryWithLogEx(string query)
        {
            int iter = 0;
            while (iter++ < 3)
            {
                try
                {
                    dbProvider.Execute(query);
                    break;
                }
                catch (Exception ex)
                {
                    logger.Error($"{query}\n{ex}");
                }
            }
        }

        private bool ExecuteQueryWithLogExWithStatus(string query)
        {
            int iter = 0;
            while (iter++ < 3)
            {
                try
                {
                    dbProvider.Execute(query);
                    return true;
                }
                catch (Exception ex)
                {
                    logger.Error($"{query}\n{ex}");
                }
            }
            return false;
        }

        private bool SaveParsedNewsToFile(CategoryNews categoryNews, ParsedFullNewsBody newsBody)
        {
            if (!Directory.Exists(pathToSaveFiles))
                Directory.CreateDirectory(pathToSaveFiles);
            string fileName = Convert.ToString(categoryNews.Id) + ".txt";
            if (File.Exists(Path.Combine(pathToSaveFiles, fileName)))
            {
                fileName = $"{Convert.ToString(categoryNews.Id)}_{Utils.DateTimeToUnix(DateTime.Now)}.txt";
            }

            SaveFileAggregator saveFile = new SaveFileAggregator(categoryNews, newsBody);

            var path = Path.Combine(pathToSaveFiles, fileName);
            try
            {
                XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(SaveFileAggregator));
                using (FileStream file = File.Create(path))
                {
                    writer.Serialize(file, saveFile);
                }
                categoryNews.ParseStatus = ParseStatus.End;
                categoryNews.FullPathToParsedTextOnDisk = path;
            }
            catch (Exception ex)
            {
                logger.Error($"Ошибка записи файла {path}\n{ex}");
                return false;
            }
            return true;
        }

        private bool SaveParsedNewsToDB(CategoryNews categoryNews, ParsedFullNewsBody newsBody)
        {
            try
            {
                FillTagsIdFromDB(newsBody);
                CheckOnEmptyTagId(newsBody);

                var queryNewsInsert = DBUtils.ParsedFullNewsBodyToInsertSql(newsBody, categoryNews.Id);
                var queryNewsAndTagsInsert = string.Empty;
                if (newsBody.Tags?.Any() == true)
                {
                    queryNewsAndTagsInsert = DBUtils.TagsWithNewsToInsertSql(newsBody.Tags, categoryNews.Id);
                }
                bool isSuccess = ExecuteQueryWithLogExWithStatus(string.Concat(queryNewsInsert, queryNewsAndTagsInsert));
                if (isSuccess)
                    categoryNews.ParseStatus = ParseStatus.End;
                else
                    categoryNews.ParseStatus = ParseStatus.Error;

            }
            catch (Exception ex)
            {
                logger.Error($"Ошибка вставки новости в базу \n{ex}");
                return false;
            }
            return true;
        }

        private void CheckOnEmptyTagId(ParsedFullNewsBody newsBody)
        {
            if (newsBody.Tags?.Any() != true)
                return;

            var tagsWithEmptyId = newsBody.Tags.Where(r => r.Id == 0);
            if (tagsWithEmptyId.Any())
            {
                InsertTags(tagsWithEmptyId);
                FillTagsIdFromDB(newsBody);
            }
        }

        private void FillTagsIdFromDB(ParsedFullNewsBody newsBody)
        {
            if (newsBody.Tags?.Any() != true)
                return;

            string tagsNames = string.Join(",", newsBody.Tags.Select(r => MySQLWrap.ToMySqlParamStat(r.Name)));
            string selectTagsQuery = $"Select `id`,`name`,`link`from tass_tags where `name` in ({tagsNames})";
            var dtTags = dbProvider.GetDataTable(selectTagsQuery);
            foreach (DataRow oneRow in dtTags.Rows)
            {
                var name = Convert.ToString(oneRow["name"]);
                var id = Convert.ToInt32(oneRow["id"]);
                var tag = newsBody.Tags.First(r => r.Name == name);
                tag.Id = id;
            }
        }

        private void InsertTags(IEnumerable<NewsTag> tagsWithEmptyId)
        {
            string queryInsertTags = DBUtils.TagsToInsertSql(tagsWithEmptyId);
            ExecuteQueryWithLogEx(queryInsertTags);
        }

        private void SaveProgressInDB(CategoryNews categoryNews)
        {
            //string queryUpdate = $"UPDATE tass_news tn SET tn.parseStatus = {MySQLWrap.ToMySqlParamStat(categoryNews.ParseStatus)}, tn.fullPath = {MySQLWrap.ToMySqlParamStat(categoryNews.FullPathToParsedTextOnDisk)}," +
            //    $" tn.dtLoad = NOW() WHERE id = {MySQLWrap.ToMySqlParamStat(categoryNews.Id)};";
            string queryUpdate = $"UPDATE tass_news tn SET tn.parseStatus = {MySQLWrap.ToMySqlParamStat(categoryNews.ParseStatus)} WHERE id = {MySQLWrap.ToMySqlParamStat(categoryNews.Id)};";
            ExecuteQueryWithLogEx(queryUpdate);
        }
        #endregion


        public void ParseNewsFromUrlTest(int id, string link)
        {
            CategoryNews categoryNews = new CategoryNews()
            {
                Id = id,
                Link = link
            };
            ParseNewsBodyByUrl(categoryNews.Link);
        }

    }
}
