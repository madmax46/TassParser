using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TassParserLib
{
    public class SaveFileAggregator
    {

        public CategoryNews CategoryNewsInfo { get; set; }
        public ParsedFullNewsBody NewsBody { get; set; }


        public SaveFileAggregator(CategoryNews categoryNewsInfo, ParsedFullNewsBody newsBody)
        {
            CategoryNewsInfo = categoryNewsInfo;
            NewsBody = newsBody;
        }
        public SaveFileAggregator()
        {
            CategoryNewsInfo = new CategoryNews();
            NewsBody = new ParsedFullNewsBody();
        }

    }
}
