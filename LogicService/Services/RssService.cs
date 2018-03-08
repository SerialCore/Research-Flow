using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using LogicService.Encapsulates;
using Windows.Web.Syndication;

namespace LogicService.Services
{
    public class RssService
    {

        public static void GetRssItems(string rssFeed, Action<IEnumerable<FeedItem>> onGetRssItemsCompleted = null, Action<string> onError = null, Action onFinally = null)
        {
            var request = HttpWebRequest.Create(rssFeed);
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
                            string content = reader.ReadToEnd();
                            List<FeedItem> rssItems = new List<FeedItem>();
                            SyndicationFeed feeds = new SyndicationFeed();
                            feeds.Load(content);
                            foreach (SyndicationItem f in feeds.Items)
                            {
                                rssItems.Add(new FeedItem
                                {
                                    Title = f.Title.Text,
                                    DOI = GetDoi(f),
                                    Authors = f.Authors,
                                    Categories = f.Categories,
                                    PublishedDate = f.PublishedDate.ToString(),
                                    AbsoluteUri = f.Links[0].Uri.AbsoluteUri,
                                    PlainSummary = WebUtility.HtmlDecode(Regex.Replace(f.Summary.Text, "<[^>]+?>", ""))
                                });
                            }
                            if (onGetRssItemsCompleted != null)
                            {
                                onGetRssItemsCompleted(rssItems);
                            }
                        }

                    }
                }
                catch (WebException webEx)
                {
                    if (onError != null)
                    {
                        onError(webEx.Message);
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

        private static string GetDoi(SyndicationItem item)
        {
            foreach (SyndicationNode node in item.ElementExtensions)
            {
                if (node.NodeName == "doi")
                {
                    return node.NodeValue;
                }
            }
            return "Doi Unavailable";
        }
    }
}
