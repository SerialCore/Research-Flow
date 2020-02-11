using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace LogicService.Data
{
    public class Topic
    {

        public string ID { get; set; }

        public string Title { get; set; }

        public DateTimeOffset Deadline { get; set; } // DateTime for only date, and DateTimeOffset for whole date and time

        public TimeSpan RemindTime { get; set; }

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (this.GetType() != obj.GetType())
                return false;

            var one = (Topic)obj;
            if (this.ID == one.ID)
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
            return ID.GetHashCode();
        }

        #endregion

        #region Tag Helper

        public static HashSet<string> TagPick(string content)
        {
            HashSet<string> tags = new HashSet<string>();

            Regex reg = new Regex(@"#(?<tag>[^#]+)#", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            Match mc = reg.Match(content);
            while (mc.Success)
            {
                tags.Add(mc.Groups["tag"].Value);
                mc = mc.NextMatch();
            }
            return tags;
        }

        public static string TagEmbed(HashSet<string> tags)
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

        public static HashSet<string> TagGenerator()
        {
            HashSet<string> tags = new HashSet<string>();

            return tags;
        }

        #endregion

    }
}
