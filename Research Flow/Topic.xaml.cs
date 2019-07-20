using LogicService.Helper;
using LogicService.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Research_Flow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Topic : Page
    {
        public Topic()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            InitializeTag();
        }

        private void InitializeTag()
        {
            List<TopicTag> tags = new List<TopicTag>();
            tags.Add(new TopicTag { Tag = "QCD" });
            tags.Add(new TopicTag { Tag = "QED" });
            tags.Add(new TopicTag { Tag = "Pedal Motion" });
            tags.Add(new TopicTag { Tag = "DNA" });
            tags.Add(new TopicTag { Tag = "AI" });
            tags.Add(new TopicTag { Tag = "Bond" });
            tags.Add(new TopicTag { Tag = "Computer" });
            tags.Add(new TopicTag { Tag = "FAT" });
            tags.Add(new TopicTag { Tag = "Hydrogen" });
            tags.Add(new TopicTag { Tag = "Halogen" });
            tags.Add(new TopicTag { Tag = "OS" });
            tags.Add(new TopicTag { Tag = "Positron" });
            tags.Add(new TopicTag { Tag = "2019" });
            tags.Add(new TopicTag { Tag = "算法" });
            tags.Add(new TopicTag { Tag = "理论" });

            Func<TopicTag, string> AlphaKey = (tag) =>
            {
                char head = tag.Tag[0];
                if (head >= '0' && head <= '9')
                    return "#";
                else if (head >= 'A' && head <= 'Z' || head >= 'a' && head <= 'z')
                    return head.ToString().ToUpper();
                else
                    return "Other";
            };

            var groups = from t in tags
                         orderby t.Tag
                         group t by AlphaKey(t);

            CollectionViewSource collectionVS = new CollectionViewSource();
            collectionVS.IsSourceGrouped = true;
            collectionVS.Source = groups;
            taglist.ItemsSource = collectionVS.View;
            tagKlist.ItemsSource = collectionVS.View.CollectionGroups;
        }
    }
  
}
