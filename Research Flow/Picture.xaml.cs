using LogicService.Application;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Research_Flow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Picture : Page
    {
        public Picture()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            InitializePicture();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

        }

        private async void InitializePicture()
        {
            IReadOnlyList<StorageFile> storageFiles = await (await LocalStorage.GetPictureFolderAsync()).GetFilesAsync();

            picStatu.Visibility = Visibility.Visible;
            picStatu.Maximum = storageFiles.Count;
            picCount.Text = storageFiles.Count.ToString();
            int index = 1;

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                foreach (StorageFile item in storageFiles)
                {
                    pictures.Add(new PictureFile { FileName = item.Name, Uri = item.Path });
                    picStatu.Value = index++;
                }
            });
            picStatu.Visibility = Visibility.Collapsed;
            picCount.Text = "";
            AdaptiveGridViewControl.ItemsSource = pictures;
            flipView.ItemsSource = pictures;
        }

        #region File Operation

        private ObservableCollection<PictureFile> pictures = new ObservableCollection<PictureFile>();

        private async void Website_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            picStatu.Visibility = Visibility.Visible;

            string[] urls = await SiteExtractor(sitelink.Text);
            picStatu.Maximum = urls.Length;
            picCount.Text = urls.Length.ToString();
            int index = 1;

            using (HttpClient client = new HttpClient())
            {
                foreach (string url in urls)
                {
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(new Uri(url));
                        if (response != null && response.StatusCode == HttpStatusCode.Ok)
                        {
                            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                            {
                                string name = url.Substring(url.LastIndexOf('/') + 1);
                                await response.Content.WriteToStreamAsync(stream);
                                StorageFile file = await (await LocalStorage.GetPictureFolderAsync()).CreateFileAsync(name, CreationCollisionOption.GenerateUniqueName);
                                await FileIO.WriteBytesAsync(file, await ConvertImagetoByte(stream));

                                pictures.Add(new PictureFile { FileName = file.Name, Uri = file.Path });
                                picStatu.Value = index++;
                            }
                        }
                    }
                    catch { }
                }
            }
            picStatu.Visibility = Visibility.Collapsed;
            picCount.Text = "";
        }

        private async void Import_Images(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            IReadOnlyList<StorageFile> imagefiles = await picker.PickMultipleFilesAsync();
            if (imagefiles.Count != 0)
            {
                picStatu.Visibility = Visibility.Visible;
                picStatu.Maximum = imagefiles.Count;
                picCount.Text = imagefiles.Count.ToString();
                int index = 1;

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    foreach (StorageFile item in imagefiles)
                    {
                        try
                        {
                            var file = await item.CopyAsync((await LocalStorage.GetPictureFolderAsync()));
                            pictures.Add(new PictureFile { FileName = file.Name, Uri = file.Path });
                            picStatu.Value = index++;
                        }
                        catch { }
                    }
                });
                picStatu.Visibility = Visibility.Collapsed;
                picCount.Text = "";
            }
        }

        private async void Export_Images(object sender, RoutedEventArgs e)
        {
            FolderPicker picker = new FolderPicker();
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            StorageFolder target = await picker.PickSingleFolderAsync();
            if (target != null)
            {
                IReadOnlyList<StorageFile> files = await (await LocalStorage.GetPictureFolderAsync()).GetFilesAsync();
                picStatu.Visibility = Visibility.Visible;
                picStatu.Maximum = files.Count;
                picCount.Text = files.Count.ToString();
                int index = 1;

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    foreach (StorageFile file in files)
                    {
                        try
                        {
                            StorageFile x = await file.CopyAsync(target);
                            picStatu.Value = index++;
                        }
                        catch { }
                    }
                });
                picStatu.Visibility = Visibility.Collapsed;
                picCount.Text = "";
            }
        }

        private async void Delete_Selected(object sender, RoutedEventArgs e)
        {
            PictureFile[] items = new PictureFile[AdaptiveGridViewControl.SelectedItems.Count];
            AdaptiveGridViewControl.SelectedItems.CopyTo(items, 0);

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                foreach (PictureFile item in items)
                {
                    StorageFile file = await (await LocalStorage.GetPictureFolderAsync()).GetFileAsync(item.FileName);
                    await file.DeleteAsync();
                    pictures.Remove(item);
                }
            });

            AdaptiveGridViewControl.SelectionMode = ListViewSelectionMode.Single;
            delete_button.Visibility = Visibility.Collapsed;
            select_all.Visibility = Visibility.Collapsed;
            select_null.Visibility = Visibility.Collapsed;
        }

        private async Task<byte[]> ConvertImagetoByte(IRandomAccessStream fileStream)
        {
            var reader = new DataReader(fileStream.GetInputStreamAt(0));
            await reader.LoadAsync((uint)fileStream.Size);
            byte[] pixels = new byte[fileStream.Size];
            reader.ReadBytes(pixels);
            return pixels;
        }

        private async Task<string[]> SiteExtractor(string url)
        {
            try
            {
                if (!url.StartsWith("http"))
                    url = url.Insert(0, "http://");
                if (!url.EndsWith("/"))
                    url += '/';

                int doubleSlash = url.IndexOf("//");
                int firstSlash = url.IndexOf("/", doubleSlash + 2);
                string domain = url.Substring(0, firstSlash);

                WebView webview = new WebView();
                webview.Source = new Uri(url);
                await Task.Delay(4000);
                string html = await webview.InvokeScriptAsync("eval", new string[] { "document.documentElement.outerHTML;" });

                Regex getImg = new Regex(@"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>", RegexOptions.IgnoreCase);
                MatchCollection matches = getImg.Matches(html);
                string[] urls = new string[matches.Count];
                int index = 0;
                foreach (Match match in matches)
                {
                    if (match.Groups["imgUrl"].Value.EndsWith(".jpg") || match.Groups["imgUrl"].Value.EndsWith(".png"))
                    {
                        if (match.Groups["imgUrl"].Value.StartsWith("/"))
                            urls[index++] = domain + match.Groups["imgUrl"].Value;
                        else
                            urls[index++] = match.Groups["imgUrl"].Value;
                    }
                }

                return urls;
            }
            catch
            {
                return new string[] { "" };
            }
        }

        #endregion

        #region Picture Operation

        private void Image_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (AdaptiveGridViewControl.SelectionMode == ListViewSelectionMode.Single)
            {
                var item = e.ClickedItem as PictureFile;
                flipView.SelectedIndex = pictures.IndexOf(item);
                albumUI.Visibility = Visibility.Collapsed;
            }
        }

        private void Image_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
            => albumUI.Visibility = Visibility.Visible;

        private void Select_Multi(object sender, RoutedEventArgs e)
        {
            if (AdaptiveGridViewControl.SelectionMode == ListViewSelectionMode.Multiple)
            {
                AdaptiveGridViewControl.SelectionMode = ListViewSelectionMode.Single;
                delete_button.Visibility = Visibility.Collapsed;
                select_all.Visibility = Visibility.Collapsed;
                select_null.Visibility = Visibility.Collapsed;
            }
            else
            {
                AdaptiveGridViewControl.SelectionMode = ListViewSelectionMode.Multiple;
                delete_button.Visibility = Visibility.Visible;
                select_all.Visibility = Visibility.Visible;
                select_null.Visibility = Visibility.Visible;
            }
        }

        private void Select_All(object sender, RoutedEventArgs e)
            => AdaptiveGridViewControl.SelectAll();

        private void Select_None(object sender, RoutedEventArgs e)
        {
            if (AdaptiveGridViewControl.SelectedItems.Count != 0)
                AdaptiveGridViewControl.SelectedItems.Clear();
        }

        #endregion

    }

    public class PictureFile
    {
        public string FileName { get; set; }

        public string Uri { get; set; }
    }
}
