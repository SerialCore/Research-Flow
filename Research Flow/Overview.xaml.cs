using LogicService.Application;
using LogicService.Data;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
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
    public sealed partial class Overview : Page
    {
        public Overview()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            InitializeMessage();
            ApplicationMessage.MessageReached += AppMessage_MessageReached;
        }

        private async void AppMessage_MessageReached(string message, ApplicationMessage.MessageType type)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                if (type == ApplicationMessage.MessageType.Chat)
                {
                    messages.Add(new MessageBot { Comment = message, IsSelf = false, Published = DateTimeOffset.Now });
                    LocalStorage.WriteJson(await LocalStorage.GetLogFolderAsync(), "messagelist", messages);
                }
            });
        }

        private async void InitializeMessage()
        {
            try
            {
                messages = await LocalStorage.ReadJsonAsync<ObservableCollection<MessageBot>>(
                    await LocalStorage.GetLogFolderAsync(), "messagelist");
            }
            catch
            {
                // for new user, remember to load default feed from file, not the follows
                messages = new ObservableCollection<MessageBot>()
                {
                    new MessageBot { Comment = "Hello", IsSelf = false },
                    new MessageBot { Comment = "for new user, remember to load default feed from file, not the follows", IsSelf = true },
                };
                LocalStorage.WriteJson(await LocalStorage.GetLogFolderAsync(), "messagelist", messages);
            }
            finally
            {
                messagelist.ItemsSource = messages;
            }
        }

        public ObservableCollection<MessageBot> messages { get; set; }
    }
}
