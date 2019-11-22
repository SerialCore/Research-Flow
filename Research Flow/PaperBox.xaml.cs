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
using LogicService.Objects;
using LogicService.Storage;

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
            var filelist = await(await LocalStorage.GetPaperFolderAsync()).GetFilesAsync();
            foreach (var file in filelist)
            {
                pdfs.Add(new PdfFile { Name = file.DisplayName.Replace(".pdf", "") });
            }
            pdflist.ItemsSource = pdfs;
        }

        #region Paper Management

        private ObservableCollection<PdfFile> pdfs = new ObservableCollection<PdfFile>();

        #endregion
    }
}
