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

        #region IsMatch

        public static bool IsMatch(string input, string pattern, RegexOptions options = RegexOptions.IgnoreCase)
        {
            return Regex.IsMatch(input, pattern, options);
        }

        public static bool IsInt(string number, RegexOptions options = RegexOptions.IgnoreCase)
        {
            string pattern = @"^[0-9]+[0-9]*$";
            return Regex.IsMatch(number, pattern, options);
        }

        public static bool IsEmail(string email, RegexOptions options = RegexOptions.IgnoreCase)
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
