using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TassParserLib
{

    public enum ParseStatus : int
    {
        NotStarted = 0,
        InProcess = 1,
        End = 2,
        Error = 3,
        NotAllow = 4
    }

    [DataContract]
    public class CategoryNewsListResponse
    {
        [DataMember(Name = "newsList")]
        public List<CategoryNews> NewsList { get; set; }
        [DataMember(Name = "lastTime")]
        public long LastTime { get; set; }
    }


    [DataContract]
    public class CategoryNews
    {

        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "isFlash")]
        public int IsFlash { get; set; }

        [DataMember(Name = "isOnline")]
        public int IsOnline { get; set; }

        [DataMember(Name = "type")]
        public string TypeNews { get; set; }

        [DataMember(Name = "mark")]
        public string Mark { get; set; }

        [DataMember(Name = "date")]
        public long Date { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "subtitle")]
        public string Subtitle { get; set; }

        [DataMember(Name = "lead")]
        public string Lead { get; set; }

        [DataMember(Name = "image")]
        public string Image { get; set; }

        [DataMember(Name = "link")]
        public string Link { get; set; }

        [DataMember(Name = "sponsor_id")]
        public int? SponsorId { get; set; }

        [DataMember(Name = "sponsor_type_id")]
        public string SponsorTypeId { get; set; }

        public int CountOfMeetings { get; set; }

        public ParseStatus ParseStatus { get; set; }

        public string FullPathToParsedTextOnDisk { get; set; }

        public DateTime? TimeParseNews { get; set; }

        public CategoryNews()
        {

        }

        public override string ToString()
        {
            string dtParse = TimeParseNews.HasValue ? TimeParseNews.Value.ToString() : string.Empty;
            return $"Id {Id} , Link {Link} , ParseStatus {ParseStatus} , DateOfNews {Utils.UnixTimeStampToDateTime(Date)} TimeParseNews {dtParse}";
        }

        public static CategoryNews FromRow(DataRow row)
        {
            CategoryNews news = new CategoryNews();
            news.Id = Convert.ToInt32(row["id"]);
            news.Date = Convert.ToInt64(row["date"]);
            news.Title = Convert.ToString(row["title"]);
            news.Link = Convert.ToString(row["link"]);
            news.Title = Convert.ToString(row["title"]);
            news.ParseStatus = (ParseStatus)Convert.ToInt32(row["parseStatus"]);

            if (row.Table.Columns.Contains("isOnline"))
                news.IsOnline = Convert.IsDBNull(row["isOnline"]) ? 0 : Convert.ToInt32(row["isOnline"]);

            if (row.Table.Columns.Contains("isFlash"))
                news.IsFlash = Convert.IsDBNull(row["isFlash"]) ? 0 : Convert.ToInt32(row["isFlash"]);

            if (row.Table.Columns.Contains("type"))
                news.TypeNews = Convert.IsDBNull(row["type"]) ? string.Empty : Convert.ToString(row["type"]);

            if (row.Table.Columns.Contains("mark"))
                news.Mark = Convert.IsDBNull(row["mark"]) ? string.Empty : Convert.ToString(row["mark"]);

            if (row.Table.Columns.Contains("subtitle"))
                news.Subtitle = Convert.IsDBNull(row["subtitle"]) ? string.Empty : Convert.ToString(row["subtitle"]);

            if (row.Table.Columns.Contains("lead "))
                news.Lead = Convert.IsDBNull(row["lead "]) ? string.Empty : Convert.ToString(row["lead "]);

            if (row.Table.Columns.Contains("sponsor_id"))
                news.SponsorId = Convert.IsDBNull(row["sponsor_id"]) ? 0 : Convert.ToInt32(row["sponsor_id"]);

            if (row.Table.Columns.Contains("sponsor_type_id"))
                news.SponsorTypeId = Convert.IsDBNull(row["sponsor_type_id"]) ? string.Empty : Convert.ToString(row["sponsor_type_id"]);

            return news;
        }
    }
}
