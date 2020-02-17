using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LogicService.Data
{
    public class ChatBlock
    {
        /// <summary>
        /// service for IdentifyMessage(ChatBlock chat)
        /// </summary>
        public static Dictionary<string, string> UserCall = new Dictionary<string, string>()
        {
            { "None", @"\S" },
            { "Alarm()", "Alarm" },
            { "Search()", "Search" },
            { "Remind()", "Remind" },
        };

        public string Comment { get; set; }

        public DateTimeOffset Published { get; set; }

        public bool IsSelf { get; set; }
    }

    public class ChatDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SelfChatDataTemplate { get; set; }

        public DataTemplate ChatDataTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var chat = item as ChatBlock;
            if (chat == null)
            {
                return this.SelfChatDataTemplate;
            }

            return chat.IsSelf ? this.SelfChatDataTemplate : this.ChatDataTemplate;
        }
    }
}
