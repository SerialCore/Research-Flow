using LogicService.Storage;
using System;
using System.Collections.ObjectModel;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Research_Flow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PdfViewer : Page
    {
        public PdfViewer()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                if (e.Parameter.GetType().Equals(typeof(string)))
                {
                    currentfile = e.Parameter as string;
                    InitializePreview();
                }
            }
        }

        private ObservableCollection<BitmapImage> pages = new ObservableCollection<BitmapImage>();

        private PdfDocument pdfDocument = null;
        private string currentfile;
        private double currentScale;

        private async void InitializePreview()
        {
            try
            {
                StorageFile file = await (await LocalStorage.GetPaperFolderAsync()).GetFileAsync(currentfile);
                pdfDocument = await PdfDocument.LoadFromFileAsync(file);
            }
            catch { }

            if (pdfDocument != null)
            {
                uint pageCount = pdfDocument.PageCount > 10 ? 10 : pdfDocument.PageCount;
                pagecount.Text = pageCount.ToString();
                pageindex.Text = "1";

                pages.Clear();
                for (uint p = 0; p < pageCount; p++)
                {
                    using (PdfPage page = pdfDocument.GetPage(p))
                    {
                        await page.PreparePageAsync();

                        InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
                        await page.RenderToStreamAsync(stream);

                        BitmapImage bitmap = new BitmapImage();
                        bitmap.DecodePixelWidth = 1920;
                        pages.Add(bitmap);
                        await bitmap.SetSourceAsync(stream);
                    }
                }
                pdfPages.ItemsSource = pages;
            }
        }

        private async void Pdf_Launch(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentfile))
                return;

            try
            {
                StorageFile file = await (await LocalStorage.GetPaperFolderAsync()).GetFileAsync(currentfile);
                await Launcher.LaunchFileAsync(file);
            }
            catch { }
        }

        private void PageIndex_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
                pdfPages.SelectedIndex = Convert.ToInt32(pageindex.Text) - 1;
        }

        private void PdfPages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            pageindex.Text = (pdfPages.SelectedIndex + 1).ToString();
        }
    }
}
