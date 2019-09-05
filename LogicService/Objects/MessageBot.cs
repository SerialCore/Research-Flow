using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LogicService.Objects
{
    public class MessageBot
    {
        public string Comment { get; set; }

        public DateTimeOffset Published { get; set; }

        public bool IsSelf { get; set; }
    }

    public class MessageItemDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SelfMessageDataTemplate { get; set; }

        public DataTemplate MessageDataTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var message = item as MessageBot;
            if (message == null)
            {
                return this.SelfMessageDataTemplate;
            }

            return message.IsSelf ? this.SelfMessageDataTemplate : this.MessageDataTemplate;
        }
    }
}
