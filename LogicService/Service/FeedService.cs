using LogicService.Data;
using LogicService.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Windows.Web.Syndication;

namespace LogicService.Service
{
    public class FeedService
    {
        public static void BeginGetFeed(string source, Action<IEnumerable<Feed>> onGetRssItemsCompleted = null, Action<string> onError = null, Action onFinally = null)
        {
            var request = HttpWebRequest.Create(source);
            request.Method = "GET";

            request.BeginGetResponse((result) =>
            {
                try
                {
                    HttpWebRequest httpWebRequest = (HttpWebRequest)result.AsyncState;
                    WebResponse webResponse = httpWebRequest.EndGetResponse(result);
                    using (Stream stream = webResponse.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            if (onGetRssItemsCompleted != null)
                            {
                                onGetRssItemsCompleted(ReadFeed(source, reader.ReadToEnd()));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (onError != null)
                    {
                        onError(e.Message);
                    }
                }
                finally
                {
                    if (onFinally != null)
                    {
                        onFinally();
                    }
                }
            }, request);
        }

        public static void GetFeed(string source, Action<IEnumerable<Feed>> onGetRssItemsCompleted = null, Action<string> onError = null, Action onFinally = null)
        {
            var request = HttpWebRequest.Create(source);
            request.Method = "GET";

            try
            {
                //await request.GetResponseAsync();
                WebResponse webResponse = request.GetResponse();
                using (Stream stream = webResponse.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        if (onGetRssItemsCompleted != null)
                        {
                            onGetRssItemsCompleted(ReadFeed(source, reader.ReadToEnd()));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (onError != null)
                {
                    onError(e.Message);
                }
            }
            finally
            {
                if (onFinally != null)
                {
                    onFinally();
                }
            }
        }

        private static List<Feed> ReadFeed(string source, string content)
        {
            List<Feed> rssItems = new List<Feed>();
            SyndicationFeed feeds = new SyndicationFeed();
            feeds.Load(content);
            foreach (SyndicationItem f in feeds.Items)
            {
                string doi = f.Id;
                string author = "";
                foreach (var ext in f.ElementExtensions)
                {
                    if (ext.NodeName.Equals("doi"))
                        doi = ext.NodeValue;
                    if (ext.NodeName.Equals("author"))
                        author = Regex.Replace(ext.NodeValue, "<[^>]+?>", "");
                }
                rssItems.Add(new Feed
                {
                    ID = HashEncode.MakeMD5(f.Links[0].Uri.AbsoluteUri),
                    Title = f.Title.Text,
                    Authors = author,
                    ArticleID = doi,
                    Published = f.PublishedDate.Year == 1601 ? DateTimeOffset.Now.ToString("yyyy-MM-dd") : f.PublishedDate.ToString("yyyy-MM-dd"),
                    Link = f.Links[0].Uri.AbsoluteUri,
                    Summary = Regex.Replace(Regex.Replace(f.Summary.Text, "<[^>]>", ""), "<[^>]+?>", " ").Replace('\n', ' ')
                });
            }
            return rssItems;
        }
    }
}