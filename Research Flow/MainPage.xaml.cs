using LogicService.Helper;
using LogicService.Services;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Research_Flow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();

            ApplicationData.Current.DataChanged += Current_DataChanged;
        }

        private void Current_DataChanged(ApplicationData sender, object args)
        {
            appMassage.Text = ApplicationService.InAppMessage;
        }

        // 消息通知方式：
        // Toast，适用于后台通知
        // Header，适用于前台通知，实时滚动消息
        // InAppNotification，适用于捕获异常
        // Dialog，适用于操作限制

        // 用户数据存储：
        // RSS数据，加密文本，同步
        // 爬虫数据，
        // 项目数据，
        // 机器学习数据，
        // 论文，
        // 截图，图片，本地 
        // 应用信息，应用设置，本地
        // 用户信息，加密文本，同步

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            Login();
            await Task.Run(async() =>
            {
                if (await Synchronization.ScanChanges())
                {
                    ApplicationService.InAppMessage = "Synchronize successfully";
                }
            });
        }

        #region NavView

        // use of anonymous class
        private readonly List<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
        {
            ("Overview", typeof(Overview)),
            ("Contact", typeof(Contact)),
            ("Topic", typeof(Topic)),
            ("Search", typeof(Search)),
            ("Learn", typeof(Learn)),
            ("Compose", typeof(Compose)),
            ("WebPage", typeof(WebPage)),
        };

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            // you can also add items in code behind
            //NavView.MenuItems.Add(new NavigationViewItemSeparator());

            // set the initial SelectedItem
            foreach (NavigationViewItemBase item in NavView.MenuItems)
            {
                if (item is NavigationViewItem && item.Tag.ToString() == "Overview")
                {
                    NavView.SelectedItem = item;
                    break;
                }
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
            if (await GraphService.ServiceLogin())
            {
                string name = await GraphService.GetDisplayName();
                string email = await GraphService.GetPrincipalName();
                BitmapImage image = new BitmapImage();
                image.UriSource = new Uri("ms-appx:///Assets/Logos/ResearchFlow_logo.jpg");
                accountName.Text = name;
                accountEmail.Text = email;
                accountPhoto.ProfilePicture = image;
            }
            else
            {
                accountName.Text = "Offline";
            }
        }

        private void Logout()
        {
            GraphService.ServiceLogout();

            ApplicationService.RemoveKey("AccountName");
            ContentFrame.IsEnabled = false;
            ContentHeader.IsEnabled = false;
            accountLogout.Content = "restart this app";
            accountName.Text = "";
            accountEmail.Text = "";
        }

        private async void AccountSync_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(async() =>
            {
                if (await Synchronization.ScanChanges())
                    ToastNotificationManager.CreateToastNotifier().Show(
                        new ToastNotification(ToastGenerator.TextToast("OneDrive", "Synchronize successfully").GetXml()));

            });
        }

        private async void AccountLogout_Click(object sender, RoutedEventArgs e)
        {
            var messageDialog = new MessageDialog("Are you sure to log out?", "Operation confirming");
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

        private async void DeleteInvokedHandler(IUICommand command)
        {
            if (ApplicationService.ContainsKey("AccountName"))
                Logout();
            else
                await CoreApplication.RequestRestartAsync(string.Empty);
        }

        private void CancelInvokedHandler(IUICommand command) { }

        #endregion

        #region Content

        private void Flyout_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
            => FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);

        private async void ScreenShot_Save(object sender, RoutedEventArgs e)
        {
            FileSavePicker picker = new FileSavePicker();
            picker.FileTypeChoices.Add("ScreenShot", new string[] { ".png" });
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            picker.SuggestedFileName = "ScreenShot-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
            StorageFile file=await picker.PickSaveFileAsync();
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
            StorageFile file = await (await LocalStorage.GetPhotoAsync()).CreateFileAsync("ScreenShot-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png", 
                CreationCollisionOption.ReplaceExisting);
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
            StorageFile file = await (await LocalStorage.GetPhotoAsync()).CreateFileAsync("ScreenShot-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png", 
                CreationCollisionOption.ReplaceExisting);
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
                    await OneDriveStorage.CreateFileAsync(await OneDriveStorage.GetPhotoAsync(), file);
                    ToastNotificationManager.CreateToastNotifier().Show(
                        new ToastNotification(ToastGenerator.TextToast("OneDrive", "Screen Shot Saved").GetXml()));
                }
                catch
                {
                    await file.CopyAsync(KnownFolders.PicturesLibrary, file.Name, NameCollisionOption.ReplaceExisting);
                    ToastNotificationManager.CreateToastNotifier().Show(
                        new ToastNotification(ToastGenerator.TextToast("Pictures Library", "Screen Shot Saved").GetXml()));
                }
            }
        }

        #endregion

    }

}
