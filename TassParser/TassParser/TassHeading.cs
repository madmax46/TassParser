using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TassParserLib
{
    public class TassHeading
    {
        public int SectionId { get; set; }
        public string URLPart { get; set; }
        public string Name { get; set; }
        public bool IsAllowParse { get; set; }

        public TassHeading(string url, string name)
        {
            URLPart = url;
            Name = name;
            IsAllowParse = true;
        }

        public TassHeading(int sectionId, string url, string name)
        {
            SectionId = sectionId;
            URLPart = url;
            Name = name;
            IsAllowParse = true;
        }

        public TassHeading(int sectionId, string uRLPart, string name, bool isAllowParse) : this(sectionId, uRLPart, name)
        {
            IsAllowParse = isAllowParse;
        }

        public override string ToString()
        {
            return $"{SectionId} |{URLPart} | {Name} | {IsAllowParse}";
        }

        public static TassHeading FromRow(DataRow row)
        {
            int sectionId = Convert.ToInt32(row[0]);
            string name = Convert.ToString(row[1]);
            string url = Convert.ToString(row[2]);
            bool isAllowParse = Convert.ToBoolean(row[3]);
            return new TassHeading(sectionId, url, name, isAllowParse);
        }
    }
}
