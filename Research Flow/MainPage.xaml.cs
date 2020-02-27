using LogicService.Application;
using LogicService.Data;
using LogicService.Service;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
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

            ConfigureUI();
            ApplicationMessage.MessageReceived += AppMessage_MessageReceived;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            FileParameter = e.Parameter as StorageFile;
            Login();
            ConfigureUpdate();
            InitializeChat();
            ForegroundTask();
            BackgroundTask();
        }

        private async void AppMessage_MessageReceived(string message, ApplicationMessage.MessageType type)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
             {
                 switch(type)
                 {
                     case ApplicationMessage.MessageType.Chat:
                         ChatBlade.IsOpen = true;
                         IdentifyChat(new ChatBlock { Comment = message, IsSelf = false, Published = DateTimeOffset.Now });
                         break;
                     case ApplicationMessage.MessageType.InApp:
                         InAppNotification.Show(message);
                         break;
                     case ApplicationMessage.MessageType.Toast:
                         ApplicationNotification.ShowTextToast("Research Flow", message);
                         break;
                 }
             });
        }
        
        private void ConfigureUI()
        {
            if (ApplicationSetting.ContainKey("HeaderColorA") && ApplicationSetting.ContainKey("HeaderColorR") &&
                ApplicationSetting.ContainKey("HeaderColorG") && ApplicationSetting.ContainKey("HeaderColorB"))
            {
                Color color = Color.FromArgb(Convert.ToByte(ApplicationSetting.HeaderColorA),
                    Convert.ToByte(ApplicationSetting.HeaderColorR),
                    Convert.ToByte(ApplicationSetting.HeaderColorG),
                    Convert.ToByte(ApplicationSetting.HeaderColorB));
                NavView.Background = new SolidColorBrush(color);
            }
        }

        private void ConfigureUpdate()
        {
            if (ApplicationInfo.ApplicationVersion.Equals("3.34.92") && !ApplicationInfo.FirstVersionInstalled.Equals("3.34.92")) // update confirmed
            {
                if (ApplicationSetting.ContainKey("IsDeveloper"))
                    ApplicationSetting.RemoveKey("IsDeveloper");
            }
        }

        private async void InitializeChat()
        {
            try
            {
                chatlist = await LocalStorage.ReadJsonAsync<ObservableCollection<ChatBlock>>(
                    await LocalStorage.GetDataFolderAsync(), "chat.list");
            }
            catch
            {
                chatlist = new ObservableCollection<ChatBlock>()
                {
                    new ChatBlock { Comment = "Hello", IsSelf = false },
                };
                LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "chat.list", chatlist);
            }
            finally
            {
                chatview.ItemsSource = chatlist;
                chattype.ItemsSource = ChatBlock.UserCall.Keys;
                chattype.SelectedIndex = 0;
            }
        }

        private void ForegroundTask()
        {
            try // if the first time to use app which has no user data
            {
                if (ApplicationInfo.IsNetworkAvailable)
                    Feed.TaskRun();
            }
            catch { }
        }

        private async void BackgroundTask()
        {
            await ApplicationTask.RegisterTopicTask();
            await ApplicationTask.RegisterTagTask();
        }

        #region NavView

        private StorageFile FileParameter;

        // use of anonymous class
        private readonly List<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
        {
            ("Overview", typeof(Overview)),
            ("TagTopic", typeof(TagTopic)),
            ("PaperBox", typeof(PaperBox)),
            ("RSS", typeof(RSS)),
            ("Search", typeof(SearchEngine)),
            ("Crawler", typeof(Crawler)),
            ("Note", typeof(Note)),
        };

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            // you can also add items in code behind
            //NavView.MenuItems.Add(new NavigationViewItemSeparator());
            if (FileParameter != null)
            {
                if (FileParameter.FileType.Equals(".rfn"))
                {
                    ContentFrame.Navigate(typeof(Note), FileParameter);
                }
            }
            else
            {
                NavView.SelectedItem = NavView.MenuItems
                    .OfType<NavigationViewItem>()
                    .First(n => n.Tag.Equals("TagTopic"));
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
            
            ContentFrame.Navigate(item.Page);
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

        #region Account

        private async void Login()
        {
            if (await GraphService.OneDriveLogin())
            {
                string name = await GraphService.GetDisplayName();
                string email = await GraphService.GetPrincipalName();
                BitmapImage image = new BitmapImage();
                image.UriSource = new Uri("ms-appx:///Images/ResearchFlow_logo.jpg");
                accountName1.Text = name;
                accountName2.Text = name;
                accountEmail.Text = email;
                accountPhoto1.ProfilePicture = image;
                accountPhoto2.ProfilePicture = image;
            }
            else
            {
                accountName1.Text = "Offline";
                accountName2.Text = "Offline";
                if (ApplicationSetting.ContainKey("AccountName"))
                    accountEmail.Text = ApplicationSetting.AccountName;
            }
        }

        private void Logout()
        {
            GraphService.OneDriveLogout();
            
            ContentFrame.IsEnabled = false;
            accountLogout.Content = "restart this app";
            accountName1.Text = "";
            accountName2.Text = "";
            accountEmail.Text = "";
        }

        private async void AccountLogout_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationSetting.ContainKey("AccountName"))
            {
                var messageDialog = new MessageDialog("Are you sure to log out?");
                messageDialog.Commands.Add(new UICommand(
                    "True",
                    new UICommandInvokedHandler(this.DeleteInvokedHandler)));
                messageDialog.Commands.Add(new UICommand(
                    "Joke",
                    new UICommandInvokedHandler(this.CancelInvokedHandler)));

                messageDialog.DefaultCommandIndex = 0;
                messageDialog.CancelCommandIndex = 1;
                await messageDialog.ShowAsync();
            }
            else
                await CoreApplication.RequestRestartAsync(string.Empty);
        }

        private void DeleteInvokedHandler(IUICommand command) => Logout();

        private void CancelInvokedHandler(IUICommand command) { }

        #endregion

        #region Content

        private bool IsHeaderColorChanged = false;

        private void Flyout_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
            => FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);

        private void Open_FlowBlade(object sender, RoutedEventArgs e)
            => FlowBlade.IsOpen = true;

        private void Open_ChatBlade(object sender, RoutedEventArgs e)
            => ChatBlade.IsOpen = true;

        private void CrawlSearch_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
            => ContentFrame.Navigate(typeof(Crawler), crawlsearch.Text);

        private async void ScreenShot_Export(object sender, RoutedEventArgs e)
        {
            FileSavePicker picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop,
                SuggestedFileName = "ScreenShot-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png",
            };
            picker.FileTypeChoices.Add("ScreenShot", new string[] { ".png" });
            StorageFile file = await picker.PickSaveFileAsync();
            if (file != null)
            {
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
                        buffer.ToArray()
                       );
                    await encode.FlushAsync();
                }
            }
        }

        private void ScreenShot_Share(object sender, RoutedEventArgs e)
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        private async void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequestDeferral deferral = args.Request.GetDeferral();

            DataRequest request = args.Request;
            request.Data.Properties.Title = "ScreenShot";
            request.Data.Properties.Description = "Share your current idea";

            var bitmap = new RenderTargetBitmap();
            StorageFile file = await LocalStorage.GetTemporaryFolder().CreateFileAsync("ScreenShot-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png");
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
                    buffer.ToArray()
                   );
                await encode.FlushAsync();
            }

            RandomAccessStreamReference imageStreamRef = RandomAccessStreamReference.CreateFromFile(file);
            request.Data.Properties.Thumbnail = imageStreamRef;
            request.Data.SetBitmap(imageStreamRef);

            deferral.Complete();
        }

        private async void ScreenShot_Upload(object sender, RoutedEventArgs e)
        {
            var bitmap = new RenderTargetBitmap();
            StorageFile file = await LocalStorage.GetTemporaryFolder().CreateFileAsync("ScreenShot-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png");
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
                    buffer.ToArray()
                   );
                await encode.FlushAsync();
            }

            if (file != null)
            {
                // confirm the app was associated with Microsoft account
                try
                {
                    await OneDriveStorage.CreateFileAsync(await OneDriveStorage.GetPictureAsync(), file);
                    ApplicationMessage.SendMessage("Screenshot Saved to OneDrive", ApplicationMessage.MessageType.Toast);
                }
                catch
                {
                    await file.CopyAsync(KnownFolders.PicturesLibrary, file.Name);
                    ApplicationMessage.SendMessage("Screenshot Saved to Pictures Library", ApplicationMessage.MessageType.Toast);
                }
            }
        }

        private void ColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            NavView.Background = new SolidColorBrush(args.NewColor);
            IsHeaderColorChanged = true;
        }

        private void ColorPicker_Closed(object sender, object e)
        {
            if (IsHeaderColorChanged)
            {
                ApplicationSetting.HeaderColorA = Convert.ToInt32(colorPicker.Color.A);
                ApplicationSetting.HeaderColorR = Convert.ToInt32(colorPicker.Color.R);
                ApplicationSetting.HeaderColorG = Convert.ToInt32(colorPicker.Color.G);
                ApplicationSetting.HeaderColorB = Convert.ToInt32(colorPicker.Color.B);
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

        private async void IdentifyChat(ChatBlock chat)
        {
            chatlist.Add(chat);
            // chat->topic request type
            LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "chat.list", chatlist);
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
