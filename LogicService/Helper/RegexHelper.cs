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

        public static string IsEmial => @"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$";

        /// <summary>
        /// Group: src
        /// </summary>
        public static string MatchImgSrc => @"(?i)<img[^>]*?\ssrc\s*=\s*(['""]?)(?<src>[^'""\s>]+)\1[^>]*>";

        /// <summary>
        /// Group: url, text
        /// </summary>
        public static string MatchALink => @"<a\shref\s*=""(?<url>[^""]*).*?>(?<text>[^<]*)</a>";

        /// <summary>
        /// Group: url, text
        /// </summary>
        public static string MatchLink => @"<link\shref\s*=""(?<url>[^""]*).*?>(?<text>[^<]*)</link>";

    }
}
