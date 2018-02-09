
using System;
using System.Net;

namespace Lykke.Service.ClientSearch.Services.FullTextSearch
{
    public static class FullTextSearchCommon
    {
        private static readonly char[] ReservedChars = new char[] { '+', '-', '&', '|', '!', '(', ')', '{', '}', '[', ']', '^', '"', '~', '*', '?', ':', '\\', '/', ',', '.', ';' };
        public const string DateTimeFormat = "yyyyMMdd";
        public const string JUMIO_NA = "N/A";

        public static string EncodeForIndex(string str)
        {
            string encodedStr = str.ToLower();
            encodedStr = WebUtility.HtmlEncode(encodedStr); // encode special symbols
            encodedStr = encodedStr.Replace("&#x", "#");
            foreach (char chToReplace in FullTextSearchCommon.ReservedChars)
            {
                encodedStr = encodedStr.Replace(chToReplace.ToString(), String.Format("#{0:X}", Convert.ToInt32(chToReplace)));
            }

            return encodedStr;
        }

        public static string DecodeFromIndex(string encodedStr)
        {
            encodedStr = encodedStr.Replace("#", "&#x");
            return WebUtility.HtmlDecode(encodedStr);
        }

    }
}
