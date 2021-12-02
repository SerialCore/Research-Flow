using LogicService.Application;
using LogicService.Data;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Research_Flow
{
    public delegate void MessageHandle(object sender);

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();

            ApplicationMessage.MessageReceived += AppMessage_MessageReceived;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            NavParameter = e.Parameter;

            if (ApplicationInfo.IsFirstUse)
            {
                ConfigureDB();
                ConfigureVersion();
            }
            //if (ApplicationSetting.ContainKey("AccountName"))
            //    Login();
            ConfigureUpdate();
        }

        private async void AppMessage_MessageReceived(MessageEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                 switch (args.Type)
                 {
                     case MessageType.Banner:
                         BannerMessage.Title = args.Title;
                         BannerMessage.Subtitle = args.Content;
                         BannerMessage.IsOpen = true;
                         break;
                     case MessageType.InApp:
                         InAppNotification.Show(args.Title + ": " + args.Content);
                         break;
                     case MessageType.Toast:
                         ApplicationNotification.ShowTextToast(args.Title, args.Content);
                         break;
                 }
            });
        }

        private void ConfigureDB()
        {
            FileList.DBInitializeTrace();
            FileList.DBInitializeList();
            Feed.DBInitialize();
        }

        private void ConfigureVersion()
        {
            ApplicationSetting.Updated = ApplicationVersion.CurrentVersion().ToString();
        }

        private void ConfigureUpdate()
        {
            // updated version must be greater than the previous published version
            // can be lighter than the next publish version
            if (ApplicationVersion.Parse(ApplicationSetting.Updated) < new ApplicationVersion(3, 42, 108, 0))
            {
                // TODO
                ApplicationSetting.Updated = "3.42.108.0";
            }
        }

        #region NavView

        private object NavParameter;

        // use of anonymous class
        private readonly List<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
        {
            ("Overview", typeof(Overview)),
            ("Feed", typeof(FeedCollector)),
            ("Bookmark", typeof(Bookmark)),
            ("Search", typeof(SearchEngine)),
            ("Topic", typeof(TopicCase)),
            ("Note", typeof(NoteDrawer)),
        };

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            // you can also add items in code behind
            //NavView.MenuItems.Add(new NavigationViewItemSeparator());
            if (NavParameter != null)
            {
                if (NavParameter.GetType().Equals(typeof(StorageFile)))
                {
                    StorageFile parameter = NavParameter as StorageFile;
                    if (parameter.FileType.Equals(".rfn"))
                    {
                        ContentFrame.Navigate(typeof(NoteDrawer), parameter);
                    }
                }
                else if (NavParameter.GetType().Equals(typeof(Uri)))
                {
                    Uri parameter = NavParameter as Uri;
                    ContentFrame.Navigate(typeof(SearchEngine), parameter.ToString());
                }
                else
                {
                    NavView.SelectedItem = NavView.MenuItems
                        .OfType<Microsoft.UI.Xaml.Controls.NavigationViewItem>()
                        .First(n => n.Tag.Equals("Overview"));
                }
            }
            else
            {
                NavView.SelectedItem = NavView.MenuItems
                    .OfType<Microsoft.UI.Xaml.Controls.NavigationViewItem>()
                    .First(n => n.Tag.Equals("Overview"));
            }
        }

        private void NavView_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                ContentFrame.Navigate(typeof(Settings), null, new DrillInNavigationTransitionInfo());
            }
            else
            {
                NavView_Navigate(args.SelectedItem as Microsoft.UI.Xaml.Controls.NavigationViewItem);
            }
        }

        private void NavView_Navigate(Microsoft.UI.Xaml.Controls.NavigationViewItem pageItem)
        {
            var item = _pages.FirstOrDefault(p => p.Tag.Equals(pageItem.Tag));

            ContentFrame.Navigate(item.Page, null, new DrillInNavigationTransitionInfo());
        }

        private void NavView_Navigated(object sender, NavigationEventArgs e)
        {
            if (ContentFrame.SourcePageType == typeof(Settings))
            {
                NavView.SelectedItem = (Microsoft.UI.Xaml.Controls.NavigationViewItem)NavView.SettingsItem;
            }
            else if (ContentFrame.SourcePageType != null)
            {
                var item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);

                NavView.SelectedItem = NavView.MenuItems
                    .OfType<Microsoft.UI.Xaml.Controls.NavigationViewItem>()
                    .First(n => n.Tag.Equals(item.Tag));
            }
        }

        private void NavView_BackRequested(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs args)
            => ContentFrame.GoBack();

        #endregion

        #region Content

        private void Flyout_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
            => FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);

        private async void ScreenShot_Upload(object sender, RoutedEventArgs e)
        {
            var file = await LocalStorage.GetPictureLibrary().CreateFileAsync("Research Flow-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png");
            var bitmap = new RenderTargetBitmap();
            await bitmap.RenderAsync(FullPage);
            var buffer = await bitmap.GetPixelsAsync();
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encode = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                encode.SetPixelData(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Ignore,
                    (uint)bitmap.PixelWidth,
                    (uint)bitmap.PixelHeight,
                    DisplayInformation.GetForCurrentView().LogicalDpi,
                    DisplayInformation.GetForCurrentView().LogicalDpi,
                    buffer.ToArray());
                await encode.FlushAsync();
            }

            ApplicationMessage.SendMessage(new MessageEventArgs { Title = "Screenshot", Content = "Saved to Pictures Library", Type = MessageType.Banner, Time = DateTimeOffset.Now });
        }

        private void FullScreen_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton button = sender as AppBarButton;
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode)
            {
                view.ExitFullScreenMode();
                button.Icon = new SymbolIcon(Symbol.FullScreen);
            }
            else
            {
                view.TryEnterFullScreenMode();
                button.Icon = new SymbolIcon(Symbol.BackToWindow);
            }
        }

        #endregion

    }
}
