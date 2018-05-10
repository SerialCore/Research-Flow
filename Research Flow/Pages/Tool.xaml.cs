using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using LogicService.Encapsulates;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Research_Flow.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Tool : Page
    {
        public ObservableCollection<StoreApp> AppList { get; set; }

        public Tool()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            // 此部分数据应放在静态数据集里
            AppList = new ObservableCollection<StoreApp>
            {
                new StoreApp { Name = "物理实验助手", AppUri = new Uri("blamderphylab:"), LogoUri = new Uri("ms-appx:///Pages/Images/Logos/phyMylab_logo.png") },
                new StoreApp { Name = "力学量采集器", AppUri = new Uri("blamderblackbox:"), LogoUri = new Uri("ms-appx:///Pages/Images/Logos/blackbox_logo.png") },
                new StoreApp { Name = "enGJFer", AppUri = new Uri("blamderengjfer:"), LogoUri = new Uri("ms-appx:///Pages/Images/Logos/enGJFer_logo.png") },
                new StoreApp { Name = "Paint Panel", AppUri = new Uri("blamderpaint:"), LogoUri = new Uri("ms-appx:///Pages/Images/Logos/paintpanel_logo.png") }
            };
            this.DataContext = this;
        }

        private async void AppList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as StoreApp;
            await Launcher.LaunchUriAsync(item.AppUri);
        }
    }
}
