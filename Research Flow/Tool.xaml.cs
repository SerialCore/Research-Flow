using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using LogicService.Objects;
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

namespace Research_Flow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Tool : Page
    {
        public List<StoreApp> AppList { get; set; }

        public Tool()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            // 此部分数据应放在静态数据集里
            AppList = new List<StoreApp>
            {
                new StoreApp{ Name = "物理实验助手", AppUri = new Uri("blamderphylab:"), LogoUri = new Uri("ms-appx:///Assets/Logos/phyMylab_logo.png") },
                new StoreApp{ Name = "力学量采集器", AppUri = new Uri("blamderblackbox:"), LogoUri = new Uri("ms-appx:///Assets/Logos/blackbox_logo.png") },
                new StoreApp{ Name = "Paint Panel", AppUri = new Uri("blamderpaint:"), LogoUri = new Uri("ms-appx:///Assets/Logos/paintpanel_logo.png") }
            };
            this.DataContext = this;
        }

        private async void AppList_ItemClick(object sender, ItemClickEventArgs e)
            => await Launcher.LaunchUriAsync((e.ClickedItem as StoreApp).AppUri);

    }

    public class StoreApp
    {

        public string Name { get; set; }

        public Uri AppUri { get; set; }

        public Uri LogoUri { get; set; }

        public string Abstract { get; set; }

    }
}
