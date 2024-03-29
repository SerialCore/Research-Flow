﻿using LogicService.Data;
using LogicService.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace LogicService.Service
{
    public class CrawlerService
    {

        public CrawlerService(string _url)
        {
            this._url = Uri.UnescapeDataString(_url);
            m_url = new Uri(_url);
            m_links = new List<Crawlable>();
            m_html = "";
            m_content = "";
            m_title = "";
            m_good = true;
            if (_url.EndsWith(".rar") || _url.EndsWith(".dat") || _url.EndsWith(".msi"))
            {
                m_good = false;
                return;
            }
        }

        #region private

        private string _url;
        private Uri m_url;
        private List<Crawlable> m_links;
        private string m_title;
        private string m_html;
        private string m_content;
        private bool m_good;
        private int m_pagesize;
        private string m_linkfilter;
        private static Dictionary<string, CookieContainer> webcookies = new Dictionary<string, CookieContainer>();

        private List<Crawlable> GetLinks()
        {
            if (m_links.Count == 0)
            {
                string pid = HashEncode.MakeMD5(_url);
                Regex[] regex = new Regex[2];
                regex[0] = new Regex(@"<a\shref\s*=""(?<url>[^""]*).*?>(?<text>[^<]*)</a>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                regex[1] = new Regex(@"<link\shref\s*=""(?<url>[^""]*).*?>(?<text>[^<]*)</link>", RegexOptions.IgnoreCase);
                for (int i = 0; i < 2; i++)
                {
                    Match match = regex[i].Match(m_html);
                    while (match.Success)
                    {
                        try
                        {
                            string url;
                            if (match.Groups["url"].Value.StartsWith('/') || match.Groups["url"].Value.StartsWith('#'))
                                url = HttpUtility.UrlDecode(new Uri(m_url, match.Groups["url"].Value).AbsoluteUri);
                            else
                                url = match.Groups["url"].Value;
                            string text = "";
                            if (i == 0) text = new Regex("(<[^>]+>)|(\\s)|( )|&|\"", RegexOptions.Multiline | RegexOptions.IgnoreCase).Replace(match.Groups["text"].Value, "");
                            m_links.Add(new Crawlable
                            {
                                ID = HashEncode.MakeMD5(url),
                                ParentID = pid,
                                Text = text,
                                Url = url,
                                Content = ""
                            });
                        }
                        catch (Exception ex) { Console.WriteLine(ex.Message); };
                        match = match.NextMatch();
                    }
                }
            }
            return m_links;
        }

        private string GetContentFromHtml(string instr, int firstN, bool withLink)
        {
            if (m_content == "")
            {
                m_content = instr.Clone() as string;
                m_content = new Regex(@"(?m)<script[^>]*>(\w|\W)*?</script[^>]*>", RegexOptions.Multiline | RegexOptions.IgnoreCase).Replace(m_content, "");
                m_content = new Regex(@"(?m)<style[^>]*>(\w|\W)*?</style[^>]*>", RegexOptions.Multiline | RegexOptions.IgnoreCase).Replace(m_content, "");
                m_content = new Regex(@"(?m)<select[^>]*>(\w|\W)*?</select[^>]*>", RegexOptions.Multiline | RegexOptions.IgnoreCase).Replace(m_content, "");
                if (!withLink) m_content = new Regex(@"(?m)<a[^>]*>(\w|\W)*?</a[^>]*>", RegexOptions.Multiline | RegexOptions.IgnoreCase).Replace(m_content, "");
                Regex objReg = new System.Text.RegularExpressions.Regex("(<[^>]+?>)| ", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                m_content = objReg.Replace(m_content, "");
                Regex objReg2 = new System.Text.RegularExpressions.Regex("(\\s)+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                m_content = objReg2.Replace(m_content, " ");
            }
            return m_content.Length > firstN ? m_content.Substring(0, firstN) : m_content;
        }

        #endregion

        public string Url
        {
            get { return m_url.AbsoluteUri; }
        }

        public string Title
        {
            get
            {
                if (m_title == "")
                {
                    Regex reg = new Regex(@"(?m)<title[^>]*>(?<title>(?:\w|\W)*?)</title[^>]*>", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                    Match mc = reg.Match(m_html);
                    if (mc.Success)
                        m_title = mc.Groups["title"].Value.Trim();
                }
                return m_title;
            }
        }

        public string Html
        {
            get { return m_html == null ? "" : m_html; }
        }

        public List<Crawlable> Links
        {
            get { return m_links.Count == 0 ? GetLinks() : m_links; }
        }

        public string Content
        {
            get { return m_content == "" ? GetContent(Int16.MaxValue) : m_content; }
        }

        public int PageSize
        {
            get { return m_pagesize; }
        }

        public List<Crawlable> InsiteLinks
        {
            get { return GetSpecialLinksByUrl("^http(s://|://)" + m_url.Host, Int16.MaxValue); }
        }

        public bool IsGood
        {
            get { return m_good; }
        }

        public string Host
        {
            get { return m_url.Host; }
        }

        public string LinkFilters
        {
            get { return m_linkfilter == null ? "" : m_linkfilter; }
            set { m_linkfilter = value; }
        }

        public string GetContent(int firstN)
        {
            return GetContentFromHtml(m_html, firstN, true);
        }

        public List<Crawlable> GetSpecialLinksByUrl(string pattern, int count = 500)
        {
            if (m_links.Count == 0) GetLinks();
            List<Crawlable> SpecialLinks = new List<Crawlable>();
            List<Crawlable>.Enumerator i;
            i = m_links.GetEnumerator();
            int cnt = 0;
            while (i.MoveNext() && cnt < count)
            {
                if (new Regex(pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase).Match(i.Current.Url).Success)
                {
                    SpecialLinks.Add(i.Current);
                    cnt++;
                }
            }
            return SpecialLinks;
        }

        public List<Crawlable> GetSpecialLinksByText(string pattern, int count = 500)
        {
            if (m_links.Count == 0) GetLinks();
            List<Crawlable> SpecialLinks = new List<Crawlable>();
            List<Crawlable>.Enumerator i;
            i = m_links.GetEnumerator();
            int cnt = 0;
            while (i.MoveNext() && cnt < count)
            {
                if (new Regex(pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase).Match(i.Current.Text).Success)
                {
                    SpecialLinks.Add(i.Current);
                    cnt++;
                }
            }
            return SpecialLinks;
        }

        public string GetSpecialWords(string pattern)
        {
            if (m_content == "") GetContent(Int16.MaxValue);
            Regex regex = new Regex(pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            Match mc = regex.Match(m_content);
            if (mc.Success)
                return mc.Groups[1].Value;
            return string.Empty;
        }

        public async void BeginGetResponse(Action<CrawlerService> onGetCrawlerCompleted, Action<string> onError = null, Action onFinally = null)
        {
            await Task.Run(() =>
            {
                try
                {
                    HttpWebRequest rqst = (HttpWebRequest)WebRequest.Create(m_url);
                    rqst.AllowAutoRedirect = true;
                    rqst.MaximumAutomaticRedirections = 3;
                    rqst.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36 Edge/18.18362";
                    rqst.KeepAlive = true;
                    rqst.Timeout = 10000;
                    lock (CrawlerService.webcookies)
                    {
                        if (CrawlerService.webcookies.ContainsKey(m_url.Host))
                            rqst.CookieContainer = CrawlerService.webcookies[m_url.Host];
                        else
                        {
                            CookieContainer cc = new CookieContainer();
                            CrawlerService.webcookies[m_url.Host] = cc;
                            rqst.CookieContainer = cc;
                        }
                    }
                    HttpWebResponse rsps = (HttpWebResponse)rqst.GetResponse();
                    Stream sm = rsps.GetResponseStream();
                    if (!rsps.ContentType.ToLower().StartsWith("text/") || rsps.ContentLength > 1 << 22)
                    {
                        rsps.Close();
                        m_good = false;
                        return;
                    }
                    Encoding cding = System.Text.Encoding.Default;
                    string contenttype = rsps.ContentType.ToLower();
                    int ix = contenttype.IndexOf("charset=");
                    if (ix != -1)
                    {
                        try
                        {
                            cding = System.Text.Encoding.GetEncoding(rsps.ContentType.Substring(ix + "charset".Length + 1));
                        }
                        catch
                        {
                            cding = Encoding.Default;
                        }
                        // need decode?
                        //m_html = HttpUtility.HtmlDecode(new StreamReader(sm, cding).ReadToEnd());
                        m_html = new StreamReader(sm, cding).ReadToEnd();
                    }
                    else
                    {
                        // need decode?
                        //m_html = HttpUtility.HtmlDecode(new StreamReader(sm, cding).ReadToEnd());
                        m_html = new StreamReader(sm, cding).ReadToEnd();
                        Regex regex = new Regex("charset=(?<cding>[^=]+)?\"", RegexOptions.IgnoreCase);
                        string strcding = regex.Match(m_html).Groups["cding"].Value;
                        try
                        {
                            cding = Encoding.GetEncoding(strcding);
                        }
                        catch
                        {
                            cding = Encoding.Default;
                        }
                        byte[] bytes = Encoding.Default.GetBytes(m_html.ToCharArray());
                        m_html = cding.GetString(bytes);
                        if (m_html.Split('?').Length > 100)
                        {
                            m_html = Encoding.Default.GetString(bytes);
                        }
                    }
                    m_pagesize = m_html.Length;
                    m_url = rsps.ResponseUri;
                    rsps.Close();

                    if (onGetCrawlerCompleted != null)
                    {
                        onGetCrawlerCompleted(this);
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
            });
        }
    }
}