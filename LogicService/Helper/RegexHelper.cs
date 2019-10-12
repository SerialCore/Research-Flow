using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LogicService.Helper
{
    public class RegexHelper
    {

        /// <summary>
        /// Group: src
        /// </summary>
        public static string MatchImgSrc => @"(?i)<img[^>]*?\ssrc\s*=\s*(['""]?)(?<src>[^'""\s>]+)\1[^>]*>";

        /// <summary>
        /// Group: url, text
        /// </summary>
        public static string MatchLinkA => @"<a\shref\s*=""(?<url>[^""]*).*?>(?<text>[^<]*)</a>";

        /// <summary>
        /// Group: url, text
        /// </summary>
        public static string MatchLink => @"<link\shref\s*=""(?<url>[^""]*).*?>(?<text>[^<]*)</link>";

    }
}
