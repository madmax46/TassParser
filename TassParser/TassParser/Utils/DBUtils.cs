using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TassParserLib
{
    public static class DBUtils
    {
        private static object SyncRoot = new object();
        private static MySQLWrap myWrap;
        public static MySQLWrap MyWrap
        {
            get
            {
                if (myWrap == null)
                {
                    lock (SyncRoot)
                    {
                        if (myWrap == null)
                        {
                            MySQLConfig config = new MySQLConfig()
                            {
                                Host = "localhost",
                                Port = 3306,
                                UserId = "root",
                                Password = "*****",
                                SslMode = "none",
                                Database = "newsparsers",
                                CharacterSet = "cp1251"
                            };
                            myWrap = new MySQLWrap(config);
                        }
                    }
                }

                return myWrap;
            }
        }

        private static object SyncRootUniver = new object();
        private static MySQLWrap myWrapUniver;
        public static MySQLWrap MyWrapUniver
        {
            get
            {
                if (myWrapUniver == null)
                {
                    lock (SyncRootUniver)
                    {
                        if (myWrapUniver == null)
                        {
                            MySQLConfig config = new MySQLConfig()
                            {
                                Host = "locaklhost",
                                Port = 3306,
                                UserId = "StockQuotes.root",
                                Password = "*****",
                                SslMode = "none",
                                Database = "StockQuotes",
                                CharacterSet = "cp1251"
                            };
                            myWrapUniver = new MySQLWrap(config);
                        }
                    }
                }

                return myWrapUniver;
            }
        }



        public static string HeadigsToInsertSql(IEnumerable<TassHeading> source)
        {
            List<string> listOfValues = new List<string>();
            foreach (var oneHeading in source)
            {
                listOfValues.Add($"({MySQLWrap.ToMySqlParamStat(oneHeading.SectionId)},{MySQLWrap.ToMySqlParamStat(oneHeading.Name)},{MySQLWrap.ToMySqlParamStat(oneHeading.URLPart)},{MySQLWrap.ToMySqlParamStat(oneHeading.IsAllowParse)})");
            }

            string query = $"insert into tass_headings (sectionId, name, url, isAllowParse) VALUES {string.Join(",", listOfValues)} ON DUPLICATE KEY UPDATE name = VALUES(`name`), url = VALUES(url), isAllowParse = VALUES(`isAllowParse`);";
            return query;
        }

        public static string CategoryNewsToInsertSql(IEnumerable<CategoryNews> source)
        {
            List<string> listOfValues = new List<string>();
            List<string> valuesOfRow = new List<string>();
            foreach (var oneNews in source)
            {
                valuesOfRow.Add(MySQLWrap.ToMySqlParamStat(oneNews.Id));
                valuesOfRow.Add(MySQLWrap.ToMySqlParamStat(oneNews.Date));
                valuesOfRow.Add(MySQLWrap.ToMySqlParamStat(oneNews.IsFlash));
                valuesOfRow.Add(MySQLWrap.ToMySqlParamStat(oneNews.IsOnline));
                valuesOfRow.Add(MySQLWrap.ToMySqlParamStat(CheckStingOnLength(oneNews.TypeNews, 255)));
                valuesOfRow.Add(MySQLWrap.ToMySqlParamStat(CheckStingOnLength(oneNews.Mark, 255)));
                valuesOfRow.Add(MySQLWrap.ToMySqlParamStat(CheckStingOnLength(oneNews.Title, 1000)));
                valuesOfRow.Add(MySQLWrap.ToMySqlParamStat(CheckStingOnLength(oneNews.Subtitle, 1000)));
                valuesOfRow.Add(MySQLWrap.ToMySqlParamStat(CheckStingOnLength(oneNews.Lead, 1000)));
                valuesOfRow.Add(MySQLWrap.ToMySqlParamStat(CheckStingOnLength(oneNews.Link, 1000)));
                valuesOfRow.Add(MySQLWrap.ToMySqlParamStat(oneNews.SponsorId));
                valuesOfRow.Add(MySQLWrap.ToMySqlParamStat(CheckStingOnLength(oneNews.SponsorTypeId, 255)));

                listOfValues.Add($"({string.Join(",", valuesOfRow)})");
                valuesOfRow.Clear();
            }

            string query = $"INSERT tass_news (id, `date`, isFlash, isOnline, type, mark, title, subtitle, lead, link, sponsor_id, sponsor_type_id) VALUES {string.Join(",", listOfValues)} ON DUPLICATE KEY UPDATE `date` = VALUES(`date`),isFlash = VALUES(`isFlash`), isOnline = VALUES(`isOnline`), type = VALUES(`type`), mark = VALUES(`mark`), title = VALUES(`title`), subtitle = VALUES(`subtitle`), lead = VALUES(`lead`),link = VALUES(`link`),  `sponsor_id` = VALUES(`sponsor_id`), sponsor_type_id = VALUES(`sponsor_type_id`);";
            return query;
        }

        public static string CategoryNewsToHeadingsAndNewsInsert(IEnumerable<CategoryNews> source, int headingId)
        {
            List<string> listOfValues = new List<string>();
            List<string> valuesOfRow = new List<string>();
            foreach (var oneNews in source)
            {
                valuesOfRow.Add(MySQLWrap.ToMySqlParamStat(headingId));
                valuesOfRow.Add(MySQLWrap.ToMySqlParamStat(oneNews.Id));
                listOfValues.Add($"({string.Join(",", valuesOfRow)})");
                valuesOfRow.Clear();
            }

            string query = $"INSERT ignore into tass_news_and_headings (headingId, newsId) VALUES {string.Join(",", listOfValues)};";
            return query;
        }

        public static string ParsedFullNewsBodyToInsertSql(ParsedFullNewsBody source, int newsId)
        {
            List<string> listOfValues = new List<string>();
            List<string> valuesOfRow = new List<string>();

            valuesOfRow.Add(MySQLWrap.ToMySqlParamStat(newsId));
            valuesOfRow.Add(MySQLWrap.ToMySqlParamStat(source.Header));
            valuesOfRow.Add(MySQLWrap.ToMySqlParamStat(source.TextBlock));
            valuesOfRow.Add(MySQLWrap.ToMySqlParamStat(source.DtLoad));
            listOfValues.Add($"({string.Join(",", valuesOfRow)})");

            string query = $"INSERT tass_parsed_news (id, `header`, `mainText`, `dtLoad`) VALUES {string.Join(",", listOfValues)} ON DUPLICATE KEY UPDATE `header` = VALUES(`header`), mainText = VALUES(`mainText`), dtLoad = VALUES(`dtLoad`);";
            return query;
        }
        public static string TagsToInsertSql(IEnumerable<NewsTag> source, bool isInsertId = false)
        {
            List<string> listOfValues = new List<string>();
            List<string> valuesOfRow = new List<string>();
            foreach (var oneTag in source)
            {
                if (isInsertId)
                    valuesOfRow.Add(MySQLWrap.ToMySqlParamStat(oneTag.Id));
                valuesOfRow.Add(MySQLWrap.ToMySqlParamStat(CheckStingOnLength(oneTag.Name, 255)));
                valuesOfRow.Add(MySQLWrap.ToMySqlParamStat(CheckStingOnLength(oneTag.URL, 1000)));
                listOfValues.Add($"({string.Join(",", valuesOfRow)})");
                valuesOfRow.Clear();
            }
            string firstColName = isInsertId ? "id," : "";
            string query = $"INSERT tass_tags ({firstColName} `name`, link) VALUES {string.Join(",", listOfValues)} ON DUPLICATE KEY UPDATE `name` = VALUES(`name`),link = VALUES(`link`);";
            return query;
        }
        public static string TagsWithNewsToInsertSql(IEnumerable<NewsTag> source, int newsId)
        {
            List<string> listOfValues = new List<string>();
            List<string> valuesOfRow = new List<string>();
            foreach (var oneTag in source)
            {
                valuesOfRow.Add(MySQLWrap.ToMySqlParamStat(newsId));
                valuesOfRow.Add(MySQLWrap.ToMySqlParamStat(oneTag.Id));
                listOfValues.Add($"({string.Join(",", valuesOfRow)})");
                valuesOfRow.Clear();
            }
            string query = $"INSERT tass_news_and_tags (`idNews`, `idTag`) VALUES {string.Join(",", listOfValues)} ON DUPLICATE KEY UPDATE `idNews` = VALUES(`idNews`),idTag = VALUES(`idTag`);";
            return query;
        }

        public static string CheckStingOnLength(string source, int maxLength)
        {
            if (string.IsNullOrEmpty(source))
                return source;

            if (source.Length > maxLength)
                return source.Substring(0, maxLength - 1);

            return source;
        }
    }
}
