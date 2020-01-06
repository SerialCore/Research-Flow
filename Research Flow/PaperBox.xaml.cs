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

        #region Pdf Management

        private ObservableCollection<string> pdfs = new ObservableCollection<string>();

        private async void Pdftree_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            var item = args.InvokedItem as string;
            var file = await(await LocalStorage.GetPaperFolderAsync()).GetFileAsync(item + ".pdf");
            await Launcher.LaunchFileAsync(file);
        }

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
