using LogicService.Application;
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
    public delegate void MessageHandle(object sender);

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();
            
            ApplicationMessage.MessageReached += AppMessage_MessageReached;
        }

        private async void AppMessage_MessageReached(string message, int span)
        {
            appMessage.Text = message;

            await Task.Delay(span * 1000);
            appMessage.Text = "";
        }

        // 消息通知方式：
        // Toast，只用于后台通知
        // Header(Event)，适用于前台通知，且用户能注意到
        // InAppNotification，只用于捕获异常
        // Dialog，适用于操作限制

        // 用户数据存储：
        // RSS数据        文本      同步
        // 爬虫数据
        // 项目数据
        // 机器学习数据
        // 论文
        // 截图           图片      本地 
        // 应用信息       应用设置   本地
        // 用户信息

        //目前的同步机制不能满足远程删除，要么使用数据库管理文件，要么在解压时对照被删除文件。
        //Quartz.NET作业调度
        //WebView存储浏览记录，保存在Log文件夹里
        //RSS列表可自定义顺序，包括手动拖拽，或者一键排序
        //Feed内容要包含时间，这样可以通知新Feed，并可以给用户呈现出一个热点、时间图
        //数据库要存哪些数据？适合存储小数据，不适合大段文字和复杂格式以及复杂的继承关系。
        //注册后台任务
        //爬虫的界面、流、存储
        //自定义关键词，项目管理
        //关键词的图表示
        //朋友圈界面，聊天界面，联系人界面
        //主页的平板化界面
        //论文管理
        //应用内搜索，文件遍历
        //收集用户信息
        //登录IP，保存在Log里
        //Url搜索引擎
        //哪些文件需要同步，扩展名
        //需不需要log文件，记录什么？
        //优化数据存储结构、加密方式，选择导出到设备
        //不同数据源的学习方法是不同的，网页爬虫，Feed统计
        //How-tos
        //x3C对接，3.0

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (GraphService.IsSignedIn)
            {
                string name = await GraphService.GetDisplayName();
                string email = await GraphService.GetPrincipalName();
                BitmapImage image = new BitmapImage();
                image.UriSource = new Uri("ms-appx:///Assets/Logos/ResearchFlow_logo.jpg");
                accountName.Text = name;
                accountEmail.Text = email;
                accountPhoto.ProfilePicture = image;

                //if (await Synchronization.ScanChanges())
                //{
                //    AppMessage.SendMessage("Synchronize successfully", AppMessage.MessageType.Bravo);
                //}
            }
            else
            {
                accountName.Text = "Offline";
                accountEmail.Text = ApplicationSetting.AccountName;
            }
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

        private void Logout()
        {
            GraphService.ServiceLogout();
            
            ContentFrame.IsEnabled = false;
            accountLogout.Content = "restart this app";
            accountName.Text = "";
            accountEmail.Text = "";
        }

        private async void AccountSync_Click(object sender, RoutedEventArgs e)
        {
            if (GraphService.IsSignedIn)
            {
                ApplicationMessage.SendMessage("Synchronizing", 3);
                if (await Synchronization.ScanChanges())
                    ApplicationMessage.SendMessage("Synchronize successfully", 5);
            }
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
            if (ApplicationSetting.KeyContain("AccountName"))
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
