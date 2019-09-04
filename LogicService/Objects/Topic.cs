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

        public DateTimeOffset StartDate { get; set; } // DateTime for only date, and DateTimeOffset for whole date and time

        public DateTimeOffset EndDate { get; set; }

        public TimeSpan RemindTime { get; set; }

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (this.GetType() != obj.GetType())
                return false;

            var one = (Topic)obj;
            if (this.Title == one.Title)
                return true;
            else
                return false;
        }

        public static bool operator ==(Topic leftHandSide, Topic rightHandSide)
        {
            if (ReferenceEquals(leftHandSide, null))
                return ReferenceEquals(rightHandSide, null);
            return (leftHandSide.Equals(rightHandSide));
        }

        public static bool operator !=(Topic leftHandSide, Topic rightHandSide)
        {
            return !(leftHandSide == rightHandSide);
        }

        public override int GetHashCode()
        {
            return Title.GetHashCode(); ;
        }

        #endregion

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
