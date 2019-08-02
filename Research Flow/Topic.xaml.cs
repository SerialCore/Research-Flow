using LogicService.Helper;
using LogicService.Objects;
using LogicService.Storage;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Research_Flow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Topic : Page
    {
        public Topic()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            InitializeTag();
        }

        private async void InitializeTag()
        {
            try
            {
                tags = await LocalStorage.ReadJsonAsync<List<string>>(
                    await LocalStorage.GetDataAsync(), "taglist");
            }
            catch
            {
                // for new user, remember to load default feed from file, not the follows
                tags = new List<string>()
                {
                    "QCD", "QED", "Pedal Motion", "DNA", "AI", "Bond", "Computer", "Hydrogen", "Halogen", "OS"
                };
                LocalStorage.WriteJson(await LocalStorage.GetDataAsync(), "taglist", tags);
            }
            finally
            {
                LoadTagView();
            }
        }

        #region Tag Management

        public List<string> tags { get; set; }

        private void LoadTagView()
        {
            Func<string, string> AlphaKey = (tag) =>
            {
                char head = tag[0];
                if (head >= '0' && head <= '9')
                    return "#";
                else if (head >= 'A' && head <= 'Z' || head >= 'a' && head <= 'z')
                    return head.ToString().ToUpper();
                else
                    return "Other";
            };

            var groups = from t in tags
                         orderby t
                         group t by AlphaKey(t);

            CollectionViewSource collectionVS = new CollectionViewSource();
            collectionVS.IsSourceGrouped = true;
            collectionVS.Source = groups;
            taglist.ItemsSource = collectionVS.View;
            tagKlist.ItemsSource = collectionVS.View.CollectionGroups;
        }

        #endregion

    }

}
