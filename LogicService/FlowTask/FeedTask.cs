using LogicService.Application;
using LogicService.Data;
using LogicService.Service;
using LogicService.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LogicService.FlowTask
{
    public class FeedTask : ForegroundTask
    {
        public delegate void TaskHandle(TaskCompletedEventArgs args);

        public event TaskHandle TaskCompleted;

        public override async void Run()
        {
            IsRunning = true;
            TaskCompletedEventArgs args = new TaskCompletedEventArgs();
            args.Task = typeof(FeedTask);

            List<FeedSource> sources = null;
            try
            {
                sources = await LocalStorage.ReadJsonAsync<List<FeedSource>>(LocalStorage.GetLocalCacheFolder(), "rss.list");
            }
            catch
            {
                sources = new List<FeedSource>();
            }

            int count = 0;
            foreach (FeedSource source in sources)
            {
                FeedService.BeginGetFeed(
                    source.Uri,
                    (items) =>
                    {
                        List<Feed> feeds = items as List<Feed>;
                        Feed.DBInsert(feeds);
                        args.Log += $"{source.Name} updated\t\n";

                        count++;
                        if (count == sources.Count)
                        {
                            IsRunning = false;
                            if (TaskCompleted != null)
                            {
                                TaskCompleted(args);
                            }
                        }
                    },
                    (exception) =>
                    {
                        ApplicationMessage.SendMessage(new MessageEventArgs { Title = "RssException", Content = exception, Type = MessageType.InApp, Time = DateTimeOffset.Now });
                        args.Log += $"{source.Name} {exception}\t\n";

                        count++;
                        if (count == sources.Count)
                        {
                            IsRunning = false;
                            if (TaskCompleted != null)
                            {
                                TaskCompleted(args);
                            }
                        }
                    }, null);
            }
        }
    }
}
