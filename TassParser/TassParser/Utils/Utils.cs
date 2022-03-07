using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TassParserLib
{
    public static class Utils
    {
        public static long DateTimeToUnix(DateTime dateTimeSource)
        {
            return ((DateTimeOffset)dateTimeSource).ToUnixTimeSeconds();
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static string ClearTextFromHtmlTags(string text)
        {
            string clearText = Regex.Replace(System.Net.WebUtility.HtmlDecode(text), "<[^>]+>|$\\s*^", string.Empty, RegexOptions.Multiline);
            return clearText;
        }
    }
}
