using LogicService.Application;
using LogicService.Data;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Popups;
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
                ApplicationConfig.ConfigureDB();
                ApplicationConfig.ConfigureVersion();
            }
            //if (ApplicationSetting.ContainKey("AccountName"))
            //    Login();
            ApplicationConfig.ConfigureUpdate();
            //InitializeChat();
        }

        private async void AppMessage_MessageReceived(ShortMessage message, ApplicationMessage.MessageType type)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
             {
                 switch (type)
                 {
                     case ApplicationMessage.MessageType.Banner:
                         BannerMessage.Title = message.Title;
                         BannerMessage.Subtitle = message.Content;
                         BannerMessage.IsOpen = true;
                         break;
                     case ApplicationMessage.MessageType.Chat:
                         ChatBlade.IsOpen = true;
                         IdentifyChat(new ChatBlock { Comment = message.Title + "\t\n" + message.Content, IsSelf = false, Published = message.Time });
                         break;
                     case ApplicationMessage.MessageType.InApp:
                         InAppNotification.Show(message.Title + ": " + message.Content);
                         break;
                     case ApplicationMessage.MessageType.Toast:
                         ApplicationNotification.ShowTextToast(message.Title, message.Content);
                         break;
                 }
             });
        }

        private async void InitializeChat()
        {
            try
            {
                chatlist = await LocalStorage.ReadJsonAsync<ObservableCollection<ChatBlock>>(LocalStorage.GetLocalCacheFolder(), "chat.list");
            }
            catch
            {
                chatlist = new ObservableCollection<ChatBlock>()
                {
                    new ChatBlock { Comment = "Hello", IsSelf = false },
                };
                LocalStorage.WriteJson(LocalStorage.GetLocalCacheFolder(), "chat.list", chatlist);
            }
            finally
            {
                chatview.ItemsSource = chatlist;
                chattype.ItemsSource = ChatBlock.UserCall.Keys;
                chattype.SelectedIndex = 0;
            }
        }

        #region NavView

        private object NavParameter;

        // use of anonymous class
        private readonly List<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
        {
            ("Overview", typeof(Overview)),
            ("Feed", typeof(FeedCollector)),
            ("Paper", typeof(PaperBox)),
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
                        .OfType<NavigationViewItem>()
                        .First(n => n.Tag.Equals("Overview"));
                }
            }
            else
            {
                NavView.SelectedItem = NavView.MenuItems
                    .OfType<NavigationViewItem>()
                    .First(n => n.Tag.Equals("Overview"));
            }
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                ContentFrame.Navigate(typeof(Settings));
            }
            else
            {
                NavView_Navigate(args.SelectedItem as NavigationViewItem);
            }
        }

        private void NavView_Navigate(NavigationViewItem pageItem)
        {
            var item = _pages.FirstOrDefault(p => p.Tag.Equals(pageItem.Tag));

            ContentFrame.Navigate(item.Page, null, new DrillInNavigationTransitionInfo());
        }

        private void NavView_Navigated(object sender, NavigationEventArgs e)
        {
            if (ContentFrame.SourcePageType == typeof(Settings))
            {
                NavView.SelectedItem = (NavigationViewItem)NavView.SettingsItem;
            }
            else if (ContentFrame.SourcePageType != null)
            {
                var item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);

                NavView.SelectedItem = NavView.MenuItems
                    .OfType<NavigationViewItem>()
                    .First(n => n.Tag.Equals(item.Tag));
            }
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
            => ContentFrame.GoBack();

        #endregion

        #region Content

        private void Flyout_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
            => FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);

        private void Open_FlowBlade(object sender, RoutedEventArgs e)
            => FlowBlade.IsOpen = true;

        private void Open_ChatBlade(object sender, RoutedEventArgs e)
            => ChatBlade.IsOpen = true;

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

            ApplicationMessage.SendMessage(new ShortMessage { Title = "Screenshot", Content = "Saved to Pictures Library", Time = DateTimeOffset.Now },
                    ApplicationMessage.MessageType.Banner);
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

        #region Chat

        private ObservableCollection<ChatBlock> chatlist = new ObservableCollection<ChatBlock>();

        private void ChatView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            chatscroll.ChangeView(null, double.MaxValue, null, true);
        }

        private void ChatType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(chattype.SelectedItem as string).Equals("None"))
                chatbox.Text = chattype.SelectedItem as string;
            else
                chatbox.Text = "";
        }

        private void IdentifyChat(ChatBlock chat)
        {
            chatlist.Add(chat);
            ChatBlade.IsOpen = true;
            // chat->topic request type
            LocalStorage.WriteJson(LocalStorage.GetLocalCacheFolder(), "chat.list", chatlist);
        }

        private void SubmitChat(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(chatbox.Text))
            {
                IdentifyChat(new ChatBlock { Comment = chatbox.Text, IsSelf = true, Published = DateTimeOffset.Now });
                chatbox.Text = "";
            }
        }

        #endregion

    }
}
