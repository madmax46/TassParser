using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TassParserLib
{
    public class JSONParamsLoadNews
    {
        // "{\"sectionId\":25,\"limit\":20,\"type\":\"all\", \"excludeNewsIds\":\"5887430,5887533,5887628,5887593\", \"imageSize\":434,\"timestamp\":1544197256}",
        public int SectionId { get; set; }
        public int Limit { get; set; }
        public string Type { get; set; }
        public List<int> ExcludeNewsIds { get; set; }
        public int imageSize { get; set; }
        public long Timestamp { get; set; }


        public override string ToString()
        {
            if (string.IsNullOrEmpty(Type))
                this.Type = "all";

            List<string> values = new List<string>();
            values.Add(CreateStringForOneParameter("sectionId", SectionId.ToString()));
            values.Add(CreateStringForOneParameter("limit", Limit.ToString()));
            values.Add(CreateStringForOneParameter("type", Type, true));
            if (ExcludeNewsIds?.Any() == true)
                values.Add(CreateStringForOneParameter("excludeNewsIds", string.Join("", ExcludeNewsIds), true));
            else
                values.Add(CreateStringForOneParameter("excludeNewsIds", "", true));

            values.Add(CreateStringForOneParameter("imageSize", imageSize.ToString()));
            values.Add(CreateStringForOneParameter("timestamp", Timestamp.ToString()));

            string retVal = "{" + string.Join(",", values) + "}";
            return retVal;
        }

        private string CreateStringForOneParameter(string name, string value, bool needAddQuotes = false)
        {
            string secondPart = needAddQuotes == true ? $"\"{value}\"" : value;
            string retVal = $"\"{name}\":{secondPart}";
            return retVal;
        }
    }
}




