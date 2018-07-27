using LogicService.Encapsulates;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
    public sealed partial class Configure : Page
    {
        public Configure()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values["Configured"] = 1;
        }

        private void Finish_config_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private async void ConfigFiles()
        {
            // RSS source
            var FeedSources = new List<FeedSource>()
            {
                new FeedSource{ID="12345678",Name="Hydrogen Bond in ACS",Uri="https://pubs.acs.org/action/showFeed?ui=0&mi=51p9f8o&type=search&feed=rss&query=%2526AllField%253DHydrogen%252BBond%2526target%253Ddefault%2526targetTab%253Dstd",Star=5,IsJournal=true},
                new FeedSource{ID="87654321",Name="Physical Review Letters",Uri="http://feeds.aps.org/rss/recent/prl.xml",Star=5,IsJournal=true},
                new FeedSource{ID="32536485",Name="科学网-数理科学",Uri="http://www.sciencenet.cn/xml/paper.aspx?di=7",Star=5,IsJournal=false}
            };
            await LocalStorage.WriteObjectAsync(await LocalStorage.GetFeedsAsync(), "RSS", FeedSources);
        }
    }
}
