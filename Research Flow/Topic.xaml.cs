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

            Func<TopicTag, string> AlphaKey = (tag) =>
            {
                return tag.Tag.Substring(0, 1).ToUpper();
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
