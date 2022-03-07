using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TassParserLib
{
    public class NewsTag
    {
        public int Id { get; set; }
        public string URL { get; set; }
        public string Name { get; set; }

        public NewsTag(int id, string url, string name) : this(url, name)
        {
            Id = id;
        }
        public NewsTag(string uRL, string name)
        {
            URL = uRL;
            Name = name;
        }
        public NewsTag()
        {
        }

    }


    public class ParsedFullNewsBody
    {
        public string Header { get; set; }

        public string TextBlock { get; set; }

        public List<NewsTag> Tags { get; set; }

        public DateTime DtLoad { get; set; }
        public ParsedFullNewsBody()
        {

        }

    }
}
