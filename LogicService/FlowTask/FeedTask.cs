using LogicService.Data;
using LogicService.Security;
using LogicService.Service;
using LogicService.Storage;
using System;
using System.Collections.Generic;

namespace LogicService.FlowTask
{
    public class FeedTask
    {

        private string _id;
        private bool _isrunning;
        private bool _isavailable;

        public FeedTask()
        {
            this._id = HashEncode.GetRandomValue();
            this._isrunning = false;
        }

        public string ID
        {
            get { return _id; }
        }

        public bool IsRunning
        {
            get { return _isrunning; }
        }

        public bool IsAvailable
        {
            get { return _isavailable; }
        }

        public async void Run()
        {
            _isrunning = true;

            List<FeedSource> FeedSources = await LocalStorage.ReadJsonAsync<List<FeedSource>>(await LocalStorage.GetDataFolderAsync(), "rss.list");

            foreach (FeedSource source in FeedSources)
            {
                RssService.GetRssItems(
                    source.Uri,
                    async (items) =>
                    {
                        List<Feed> feeds = items as List<Feed>;
                        Feed.DBInsert(feeds);
                        source.LastUpdateTime = DateTime.Now;
                        LocalStorage.WriteJson(await LocalStorage.GetDataFolderAsync(), "rss.list", FeedSources);

                        LocalStorage.GeneralLogAsync<Feed>("FeedTask.log", "feed updated-" + source.Name);
                        _isrunning = false;
                    },
                    (exception) =>
                    {
                        LocalStorage.GeneralLogAsync<Feed>("FeedTask.log", exception + "-" + source.Name);
                        _isrunning = false;
                    }, null);
            }
        }

    }
}
