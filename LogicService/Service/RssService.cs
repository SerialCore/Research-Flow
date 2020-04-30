using LogicService.Data;
using LogicService.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Windows.Web.Syndication;

namespace LogicService.Service
{
    public class RssService
    {

        /// <summary>
        /// run as a thread
        /// </summary>
        /// <param name="rssFeed"></param>
        /// <param name="onGetRssItemsCompleted"></param>
        /// <param name="onError"></param>
        /// <param name="onFinally"></param>
        public static void BeginGetFeed(string rssFeed, Action<IEnumerable<Feed>> onGetRssItemsCompleted = null, Action<string> onError = null, Action onFinally = null)
        {
            var request = HttpWebRequest.Create(rssFeed);
            request.Method = "GET";

            request.BeginGetResponse((result) => // stop here in backgroundtask
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
                            string parentID = HashEncode.MakeMD5(rssFeed);
                            List<Feed> rssItems = new List<Feed>();
                            SyndicationFeed feeds = new SyndicationFeed();
                            feeds.Load(content);
                            foreach (SyndicationItem f in feeds.Items)
                            {
                                rssItems.Add(new Feed
                                {
                                    ID = HashEncode.MakeMD5(f.Links[0].Uri.AbsoluteUri),
                                    ParentID = parentID,
                                    Title = f.Title.Text,
                                    Published = f.PublishedDate.ToString(),
                                    Link = f.Links[0].Uri.AbsoluteUri,
                                    Summary = WebUtility.HtmlDecode(Regex.Replace(f.Summary.Text, "<[^>]+?>", "")),
                                    Tags = "",
                                    Nodes = f.GetXmlDocument(SyndicationFormat.Rss20).GetXml(),
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

        public static void GetFeed(string rssFeed, Action<IEnumerable<Feed>> onGetRssItemsCompleted = null, Action<string> onError = null, Action onFinally = null)
        {
            var request = HttpWebRequest.Create(rssFeed);
            request.Method = "GET";

            try
            {
                using (Stream stream = request.GetResponse().GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string content = reader.ReadToEnd();
                        string parentID = HashEncode.MakeMD5(rssFeed);
                        List<Feed> rssItems = new List<Feed>();
                        SyndicationFeed feeds = new SyndicationFeed();
                        feeds.Load(content);
                        foreach (SyndicationItem f in feeds.Items)
                        {
                            rssItems.Add(new Feed
                            {
                                ID = HashEncode.MakeMD5(f.Links[0].Uri.AbsoluteUri),
                                ParentID = parentID,
                                Title = f.Title.Text,
                                Published = f.PublishedDate.ToString(),
                                Link = f.Links[0].Uri.AbsoluteUri,
                                Summary = WebUtility.HtmlDecode(Regex.Replace(f.Summary.Text, "<[^>]+?>", "")),
                                Tags = "",
                                Nodes = f.GetXmlDocument(SyndicationFormat.Rss20).GetXml(),
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
        }
    }
}