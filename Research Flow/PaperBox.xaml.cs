using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using LogicService.Data;
using LogicService.Storage;
using Windows.System;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using LogicService.Application;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Research_Flow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PaperBox : Page
    {
        public PaperBox()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            InitializePaper();
        }

        private async void InitializePaper()
        {
            pdfs.Clear();
            var filelist = await (await LocalStorage.GetPaperFolderAsync()).GetFilesAsync();
            foreach (var file in filelist)
            {
                pdfs.Add(file.DisplayName.Replace(".pdf", ""));
            }
            pdftree.ItemsSource = pdfs;
        }

        #region File Management

        private ObservableCollection<string> pdfs = new ObservableCollection<string>();

        private void Pdftree_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            var item = args.InvokedItem as string;
            pdfname.Text = item + ".pdf";
            PdfPreview(pdfname.Text);
        }

        #endregion

        #region Pdf Operation

        private ObservableCollection<BitmapImage> pages = new ObservableCollection<BitmapImage>();

        PdfDocument pdfDocument = null;

        private async void PdfPreview(string filename)
        {
            try
            {
                StorageFile file = await (await LocalStorage.GetPaperFolderAsync()).GetFileAsync(filename);
                pdfDocument = await PdfDocument.LoadFromFileAsync(file);
            }
            catch (Exception exception)
            {
                ApplicationMessage.SendMessage("PdfException: " + exception.Message, ApplicationMessage.MessageType.InApp);
            }

            if (pdfDocument != null)
            {
                uint pageCount = pdfDocument.PageCount > 20 ? 20 : pdfDocument.PageCount;
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
                        bitmap.DecodePixelWidth = 1360;
                        pages.Add(bitmap);
                        await bitmap.SetSourceAsync(stream);
                    }
                }
                pdfPages.ItemsSource = pages;
            }
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

        private async void Pdf_Launch(object sender, RoutedEventArgs e)
        {
            try
            {
                StorageFile file = await (await LocalStorage.GetPaperFolderAsync()).GetFileAsync(pdfname.Text);
                await Launcher.LaunchFileAsync(file);
            }
            catch (Exception exception)
            {
                ApplicationMessage.SendMessage("PdfException: " + exception.Message, ApplicationMessage.MessageType.InApp);
            }
        }

        #endregion

        #region Paper Management

        private void Pdf_Detail(object sender, RoutedEventArgs e) => pdfpanel.IsPaneOpen = !pdfpanel.IsPaneOpen;

        /// <summary>
        /// will be recorded
        /// </summary>
        private void ArchiveThisPaper()
        {
            // if user want to sync this paper between devices, they will have the right to make the choice
            // choosing synchronization is equals to choosing registing on database
        }

        #endregion

    }
}
