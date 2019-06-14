using LogicService.Objects;
using LogicService.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace LogicService.Services
{
    public class CrawlerService
    {
        #region 私有成员
        private Uri m_uri;  //url
        private List<Crawlable> m_links;  //此网页上的链接
        private string m_title;    //标题
        private string m_html;     //HTML代码
        private string m_outstr;    //网页可输出的纯文本
        private bool m_good;      //网页是否可用
        private int m_pagesize;    //网页的大小
        private static Dictionary<string, CookieContainer> webcookies = new Dictionary<string, CookieContainer>();//存放所有网页的Cookie
        private string m_post;         // 此网页的登陆页需要的POST数据
        private string m_loginurl;     // 此网页的登陆页
        #endregion

        #region 属性
        /// <summary>
        /// 通过此属性可获得本网页的网址，只读
        /// </summary>
        public string URL
        {
            get
            {
                return m_uri.AbsoluteUri;
            }
        }
        /// <summary>
        /// 通过此属性可获得本网页的标题，只读
        /// </summary>
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
        public string M_html
        {
            get
            {
                if (m_html == null)
                {
                    m_html = "";
                }
                return m_html;
            }
        }
        /// <summary>
        /// 此属性获得本网页的所有链接信息，只读
        /// </summary>
        public List<Crawlable> Links
        {
            get
            {
                if (m_links.Count == 0) getLinks();
                return m_links;
            }
        }
        /// <summary>
        /// 此属性返回本网页的全部纯文本信息，只读
        /// </summary>
        public string Context
        {
            get
            {
                if (m_outstr == "") getContext(Int16.MaxValue);
                return m_outstr;
            }
        }
        /// <summary>
        /// 此属性获得本网页的大小
        /// </summary>
        public int PageSize
        {
            get
            {
                return m_pagesize;
            }
        }
        /// <summary>
        /// 此属性获得本网页的所有站内链接
        /// </summary>
        public List<Crawlable> InsiteLinks
        {
            get
            {
                return getSpecialLinksByUrl("^http://" + m_uri.Host, Int16.MaxValue);
            }
        }
        /// <summary>
        /// 此属性表示本网页是否可用
        /// </summary>
        public bool IsGood
        {
            get
            {
                return m_good;
            }
        }
        /// <summary>
        /// 此属性表示网页的所在的网站
        /// </summary>
        public string Host
        {
            get
            {
                return m_uri.Host;
            }
        }
        #endregion
        /// <summary>
        /// 从HTML代码中分析出链接信息
        /// </summary>
        /// <returns>List<Link></returns>
        private List<Crawlable> getLinks()
        {
            if (m_links.Count == 0)
            {
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
                            if (match.Groups["url"].Value.StartsWith('/'))
                                url = HttpUtility.UrlDecode(new Uri(m_uri, match.Groups["url"].Value).AbsoluteUri);
                            else
                                url = match.Groups["url"].Value;
                            string text = "";
                            if (i == 0) text = new Regex("(<[^>]+>)|(\\s)|( )|&|\"", RegexOptions.Multiline | RegexOptions.IgnoreCase).Replace(match.Groups["text"].Value, "");
                            m_links.Add(new Crawlable
                            {
                                ID = HashEncode.MakeMD5(url),
                                Text = text,
                                Url = url,
                            });
                        }
                        catch (Exception ex) { Console.WriteLine(ex.Message); };
                        match = match.NextMatch();
                    }
                }
            }
            return m_links;
        }
        /// <summary>
        /// 此私有方法从一段HTML文本中提取出一定字数的纯文本
        /// </summary>
        /// <param name="instr">HTML代码</param>
        /// <param name="firstN">提取从头数多少个字</param>
        /// <param name="withLink">是否要链接里面的字</param>
        /// <returns>纯文本</returns>
        private string getFirstNchar(string instr, int firstN, bool withLink)
        {
            if (m_outstr == "")
            {
                m_outstr = instr.Clone() as string;
                m_outstr = new Regex(@"(?m)<script[^>]*>(\w|\W)*?</script[^>]*>", RegexOptions.Multiline | RegexOptions.IgnoreCase).Replace(m_outstr, "");
                m_outstr = new Regex(@"(?m)<style[^>]*>(\w|\W)*?</style[^>]*>", RegexOptions.Multiline | RegexOptions.IgnoreCase).Replace(m_outstr, "");
                m_outstr = new Regex(@"(?m)<select[^>]*>(\w|\W)*?</select[^>]*>", RegexOptions.Multiline | RegexOptions.IgnoreCase).Replace(m_outstr, "");
                if (!withLink) m_outstr = new Regex(@"(?m)<a[^>]*>(\w|\W)*?</a[^>]*>", RegexOptions.Multiline | RegexOptions.IgnoreCase).Replace(m_outstr, "");
                Regex objReg = new System.Text.RegularExpressions.Regex("(<[^>]+?>)| ", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                m_outstr = objReg.Replace(m_outstr, "");
                Regex objReg2 = new System.Text.RegularExpressions.Regex("(\\s)+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                m_outstr = objReg2.Replace(m_outstr, " ");
            }
            return m_outstr.Length > firstN ? m_outstr.Substring(0, firstN) : m_outstr;
        }
        #region 公有文法
        /// <summary>
        /// 此公有方法提取网页中一定字数的纯文本，包括链接文字
        /// </summary>
        /// <param name="firstN">字数</param>
        /// <returns></returns>
        public string getContext(int firstN)
        {
            return getFirstNchar(m_html, firstN, true);
        }
        /// <summary>
        /// 此公有方法从本网页的链接中提取一定数量的链接，该链接的URL满足某正则式
        /// </summary>
        /// <param name="pattern">正则式</param>
        /// <param name="count">返回的链接的个数</param>
        /// <returns>List<Link></returns>
        public List<Crawlable> getSpecialLinksByUrl(string pattern, int count)
        {
            if (m_links.Count == 0) getLinks();
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
        /// <summary>
        /// 此公有方法从本网页的链接中提取一定数量的链接，该链接的文字满足某正则式
        /// </summary>
        /// <param name="pattern">正则式</param>
        /// <param name="count">返回的链接的个数</param>
        /// <returns>List<Link></returns>
        public List<Crawlable> getSpecialLinksByText(string pattern, int count)
        {
            if (m_links.Count == 0) getLinks();
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
        /// <summary>
        /// 这公有方法提取本网页的纯文本中满足某正则式的文字 by 何问起
        /// </summary>
        /// <param name="pattern">正则式</param>
        /// <returns>返回文字</returns>
        public string getSpecialWords(string pattern)
        {
            if (m_outstr == "") getContext(Int16.MaxValue);
            Regex regex = new Regex(pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            Match mc = regex.Match(m_outstr);
            if (mc.Success)
                return mc.Groups[1].Value;
            return string.Empty;
        }
        #endregion
        #region 构造函数
        private void Init(string _url)
        {
            try
            {
                m_uri = new Uri(_url);
                m_links = new List<Crawlable>();
                m_html = "";
                m_outstr = "";
                m_title = "";
                m_good = true;
                if (_url.EndsWith(".rar") || _url.EndsWith(".dat") || _url.EndsWith(".msi"))
                {
                    m_good = false;
                    return;
                }
                HttpWebRequest rqst = (HttpWebRequest)WebRequest.Create(m_uri);
                rqst.AllowAutoRedirect = true;
                rqst.MaximumAutomaticRedirections = 3;
                rqst.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36 Edge/18.18362";
                rqst.KeepAlive = true;
                rqst.Timeout = 10000;
                lock (CrawlerService.webcookies)
                {
                    if (CrawlerService.webcookies.ContainsKey(m_uri.Host))
                        rqst.CookieContainer = CrawlerService.webcookies[m_uri.Host];
                    else
                    {
                        CookieContainer cc = new CookieContainer();
                        CrawlerService.webcookies[m_uri.Host] = cc;
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
                    //该处视情况而定 有的需要解码
                    //m_html = HttpUtility.HtmlDecode(new StreamReader(sm, cding).ReadToEnd());
                    m_html = new StreamReader(sm, cding).ReadToEnd();
                }
                else
                {
                    //该处视情况而定 有的需要解码
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
                m_uri = rsps.ResponseUri;
                rsps.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public CrawlerService(string _url)
        {
            string uurl = "";
            try
            {
                uurl = Uri.UnescapeDataString(_url);
                _url = uurl;
            }
            catch { };
            Init(_url);
        }

        public CrawlerService(string _url, string _loginurl, string _post)
        {
            string uurl = "";
            try
            {
                uurl = Uri.EscapeDataString(_url);
                _url = uurl;
            }
            catch { };
            if (_loginurl.Trim() == "" || _post.Trim() == "" || CrawlerService.webcookies.ContainsKey(new Uri(_url).Host))
            {
                Init(_url);
            }
            else
            {
                #region 登陆

                string indata = _post;
                m_post = _post;
                m_loginurl = _loginurl;
                byte[] bytes = Encoding.Default.GetBytes(_post);
                CookieContainer myCookieContainer = new CookieContainer();
                try
                {
                    //新建一个CookieContainer来存放Cookie集合
                    HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(_loginurl);
                    //新建一个HttpWebRequest
                    myHttpWebRequest.ContentType = "application/x-www-form-urlencoded";
                    myHttpWebRequest.AllowAutoRedirect = false;
                    myHttpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 5.01; Windows NT 5.0)";
                    myHttpWebRequest.Timeout = 60000;
                    myHttpWebRequest.KeepAlive = true;
                    myHttpWebRequest.ContentLength = bytes.Length;
                    myHttpWebRequest.Method = "POST";
                    myHttpWebRequest.CookieContainer = myCookieContainer;
                    //设置HttpWebRequest的CookieContainer为刚才建立的那个myCookieContainer
                    Stream myRequestStream = myHttpWebRequest.GetRequestStream();
                    myRequestStream.Write(bytes, 0, bytes.Length);
                    myRequestStream.Close();
                    HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                    foreach (Cookie ck in myHttpWebResponse.Cookies)
                    {
                        myCookieContainer.Add(ck);
                    }
                    myHttpWebResponse.Close();
                }
                catch
                {
                    Init(_url);
                    return;
                }

                #endregion

                #region 登陆后再访问页面

                try
                {
                    m_uri = new Uri(_url);
                    m_links = new List<Crawlable>();
                    m_html = "";
                    m_outstr = "";
                    m_title = "";
                    m_good = true;
                    if (_url.EndsWith(".rar") || _url.EndsWith(".dat") || _url.EndsWith(".msi"))
                    {
                        m_good = false;
                        return;
                    }
                    HttpWebRequest rqst = (HttpWebRequest)WebRequest.Create(m_uri);
                    rqst.AllowAutoRedirect = true;
                    rqst.MaximumAutomaticRedirections = 3;
                    rqst.UserAgent = "Mozilla/4.0 (compatible; MSIE 5.01; Windows NT 5.0)";
                    rqst.KeepAlive = true;
                    rqst.Timeout = 30000;
                    rqst.CookieContainer = myCookieContainer;
                    lock (CrawlerService.webcookies)
                    {
                        CrawlerService.webcookies[m_uri.Host] = myCookieContainer;
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
                    int ix = rsps.ContentType.ToLower().IndexOf("charset=");
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
                    }

                    m_html = new StreamReader(sm, cding).ReadToEnd();

                    m_pagesize = m_html.Length;
                    m_uri = rsps.ResponseUri;
                    rsps.Close();
                }
                catch (Exception ex)
                {
                    m_good = false;
                    throw ex;
                }

                #endregion
            }
        }
        #endregion
    }
}