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

        /// <summary>
        /// Group: tag
        /// </summary>
        public static string MatchTag => @"#(?<tag>[^#]+)#";

        public static Dictionary<string, string> LinkFilter = new Dictionary<string, string>()
        {
            { "Text: NotEmpty", @"\S" },
            { "Text: Has=", "" },
            { "Text: HasPDF", "pdf" },
            { "Url: Has=", "" },
            { "Url: HasDoi", "doi" },
            { "Url: HasAbs", "abs" },
            { "Url: HasPDF", @"(.pdf\z)|(/pdf/)" },
            { "Url: Insite", "" }, // for tag only, not truely dictionary
        };

    }
}
