using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LogicService.Objects
{
    public class Topic
    {

        public string Title { get; set; }

        public DateTime StartData { get; set; }

        public DateTime EndDate { get; set; }

        #region Tag Management

        public static List<string> TagPicker(string content)
        {
            List<string> tags = new List<string>();

            Regex reg = new Regex(@"#(?<tag>(?:\w|\W)#)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            Match mc = reg.Match(content);
            while (mc.Success)
            {
                tags.Add(mc.Groups["tag"].Value);
            }
            return tags;
        }

        public static string TagEmbed(List<string> tags)
        {
            StringBuilder embed = new StringBuilder();
            foreach (string tag in tags)
            {
                embed.Append("#");
                embed.Append(tag);
                embed.Append("#");
            }
            return embed.ToString();
        }

        #endregion

    }
}
