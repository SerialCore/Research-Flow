using LogicService.Encapsulates;
using LogicService.Services;
using LogicService.Storage;
using LogicService.Security;
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
using System.Collections.ObjectModel;

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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("AccountName"))
                Login_Tapped(null, null);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values["Configured"] = 1;
        }

        private async void Login_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (await GraphService.ServiceLogin())
            {
                accountStatu.Text = await GraphService.GetPrincipalName();
                ApplicationData.Current.LocalSettings.Values["AccountName"] = await GraphService.GetPrincipalName();
                finish_config.IsEnabled = true;

                ConfigureFile();
            }
            else
            {

            }
        }

        private async void ConfigureFile()
        {
            try
            {
                if (await Synchronization.DownloadAll())
                    finish_config.IsEnabled = true;
            }
            catch
            {
                var FeedSources = new ObservableCollection<FeedSource>()
                {
                    new FeedSource{ID=TripleDES.MakeMD5("https://pubs.acs.org/action/showFeed?ui=0&mi=51p9f8o&type=search&feed=rss&query=%2526AllField%253DHydrogen%252BBond%2526target%253Ddefault%2526targetTab%253Dstd"),
                        Name="Hydrogen Bond in ACS",Uri="https://pubs.acs.org/action/showFeed?ui=0&mi=51p9f8o&type=search&feed=rss&query=%2526AllField%253DHydrogen%252BBond%2526target%253Ddefault%2526targetTab%253Dstd",MaxCount=50,DaysforUpdate=5,Star=5,IsJournal=true},
                    new FeedSource{ID=TripleDES.MakeMD5("https://pubs.acs.org/action/showFeed?ui=0&mi=51p9f8o&type=search&feed=rss&query=%2526AllField%253DPedal%252BMotion%2526target%253Ddefault%2526targetTab%253Dstd"),
                        Name="Pedal Motion in ACS",Uri="https://pubs.acs.org/action/showFeed?ui=0&mi=51p9f8o&type=search&feed=rss&query=%2526AllField%253DPedal%252BMotion%2526target%253Ddefault%2526targetTab%253Dstd",MaxCount=50,DaysforUpdate=5,Star=5,IsJournal=true},
                    new FeedSource{ID=TripleDES.MakeMD5("https://pubs.acs.org/action/showFeed?ui=0&mi=51p9f8o&type=search&feed=rss&query=%2526field1%253DContrib%2526target%253Ddefault%2526targetTab%253Dstd%2526text1%253DPaul%252BL.%252BA.%252BPopelier"),
                        Name="Paul L. A. Popelier in ACS",Uri="https://pubs.acs.org/action/showFeed?ui=0&mi=51p9f8o&type=search&feed=rss&query=%2526field1%253DContrib%2526target%253Ddefault%2526targetTab%253Dstd%2526text1%253DPaul%252BL.%252BA.%252BPopelier",MaxCount=50,DaysforUpdate=5,Star=5,IsJournal=true},
                    new FeedSource{ID=TripleDES.MakeMD5("https://pubs.acs.org/action/showFeed?ui=0&mi=51p9f8o&type=search&feed=rss&query=%2526AllField%253DFuyang%252BLi%2526target%253Ddefault%2526targetTab%253Dstd"),
                        Name="Fuyang Li in ACS",Uri="https://pubs.acs.org/action/showFeed?ui=0&mi=51p9f8o&type=search&feed=rss&query=%2526AllField%253DFuyang%252BLi%2526target%253Ddefault%2526targetTab%253Dstd",MaxCount=50,DaysforUpdate=5,Star=5,IsJournal=true},
                    new FeedSource{ID=TripleDES.MakeMD5("http://feeds.aps.org/rss/recent/prl.xml"),
                        Name="Physical Review Letters",Uri="http://feeds.aps.org/rss/recent/prl.xml",MaxCount=50,DaysforUpdate=5,Star=5,IsJournal=true},
                    new FeedSource{ID=TripleDES.MakeMD5("http://www.sciencenet.cn/xml/paper.aspx?di=7"),
                        Name="科学网-数理科学",Uri="http://www.sciencenet.cn/xml/paper.aspx?di=7",MaxCount=50,DaysforUpdate=5,Star=5,IsJournal=false}
                };
                await LocalStorage.WriteObjectAsync(await LocalStorage.GetFeedsAsync(), "RSS", FeedSources);
            }
        }

        private void Finish_config_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

    }
}
