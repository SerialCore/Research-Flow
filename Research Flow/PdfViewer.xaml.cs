using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System.Collections.ObjectModel;

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
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        public ObservableCollection<BitmapImage> PdfPages { get; set; } = new ObservableCollection<BitmapImage>();

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            StorageFile file = e.Parameter as StorageFile;
            if (file != null)
            {
                try
                {
                    PdfDocument doc = await PdfDocument.LoadFromFileAsync(file);
                    LoadPdf(doc);
                }
                catch (Exception ex)
                {
                    InAppNotification.Show(ex.Message);
                }
            }

            InitializePdf();

            var pdfile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Resources/paper.txt"));
            PdfDocument a = await PdfDocument.LoadFromFileAsync(pdfile);
            LoadPdf(a);
        }

        private void InitializePdf()
        {
            // load doc list or other static settings
        }

        private async void LoadPdf(PdfDocument doc)
        {
            PdfPages.Clear();

            for (uint i = 0; i < 5; i++)
            {
                BitmapImage image = new BitmapImage();

                var page = doc.GetPage(i);

                using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                {
                    await page.RenderToStreamAsync(stream);
                    await image.SetSourceAsync(stream);
                }

                PdfPages.Add(image);
            }

            pagelist.ItemsSource = PdfPages;
        }
    }
}
