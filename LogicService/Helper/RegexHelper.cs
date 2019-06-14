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

        private static string regexHref => "href\\s*=\\s*(?:\"(?<1>[^\"]*)\"|(?<1>\\S+))";

        #region Regex

        public static Match MatchHref(string content)
        {
            Regex regex = new Regex(regexHref, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            return regex.Match(content);
        }

        public static Match MatchImgSrc(string content)
        {
            Regex regex = new Regex(@"(?i)<img[^>]*?\ssrc\s*=\s*(['""]?)(?<src>[^'""\s>]+)\1[^>]*>");
            return regex.Match(content);
        }

        #endregion

        #region IsMatch

        public static bool IsMatch(string input, string pattern, RegexOptions options = RegexOptions.Multiline | RegexOptions.IgnoreCase)
        {
            return Regex.IsMatch(input, pattern, options);
        }

        public static bool IsInt(string number, RegexOptions options = RegexOptions.Multiline | RegexOptions.IgnoreCase)
        {
            string pattern = @"^[0-9]+[0-9]*$";
            return Regex.IsMatch(number, pattern, options);
        }

        public static bool IsEmail(string email, RegexOptions options = RegexOptions.Multiline | RegexOptions.IgnoreCase)
        {
            string pattern = @"^([0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$";
            return Regex.IsMatch(email, pattern, options);
        }

        #endregion

        #region Replace

        public static string Replace(string input, string pattern, string alter, RegexOptions options = RegexOptions.IgnoreCase)
        {
            return Regex.Replace(input, pattern, alter, options);
        }

        #endregion

    }
}
