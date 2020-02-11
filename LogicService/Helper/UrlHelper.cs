using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace LogicService.Helper
{
    public class UrlHelper
    {
        static System.Text.Encoding encoding = System.Text.Encoding.UTF8;

        #region URL��64λ����
        public static string Base64Encrypt(string sourceUrl)
        {
            string eurl = WebUtility.UrlEncode(sourceUrl);
            eurl = Convert.ToBase64String(encoding.GetBytes(eurl));
            return eurl;
        }
        #endregion

        #region URL��64λ����
        public static string Base64Decrypt(string eStr)
        {        
            if (!IsBase64(eStr))
            {
                return eStr;
            }
            byte[] buffer = Convert.FromBase64String(eStr);
            string sourceUrl = encoding.GetString(buffer);
            sourceUrl = WebUtility.UrlDecode(sourceUrl);
            return sourceUrl;
        }
        /// <summary>
        /// �Ƿ���Base64�ַ���
        /// </summary>
        /// <param name="eStr"></param>
        /// <returns></returns>
        public static bool IsBase64(string eStr)
        {
            if ((eStr.Length % 4) != 0)
            {
                return false;
            }
            if (!Regex.IsMatch(eStr, "^[A-Z0-9/+=]*$", RegexOptions.IgnoreCase))
            {
                return false;
            }
            return true;
        }
        #endregion

        /// <summary>
        /// ���URL����
        /// </summary>
        public static string AddParam(string url, string paramName, string value)
        {
            Uri uri = new Uri(url);
            if (string.IsNullOrEmpty(uri.Query))
            {
                string eval = WebUtility.UrlEncode(value);
                return String.Concat(url, "?" + paramName + "=" + eval);
            }
            else
            {
                string eval = WebUtility.UrlEncode(value);
                return String.Concat(url, "&" + paramName + "=" + eval);
            }
        }
        /// <summary>
        /// ����URL����
        /// </summary>
        public static string UpdateParam(string url, string paramName, string value)
        {
            string keyWord = paramName+"=";
            int index = url.IndexOf(keyWord)+keyWord.Length;
            int index1 = url.IndexOf("&", index);
            if (index1 == -1)
            {
                url = url.Remove(index, url.Length - index);
                url = string.Concat(url, value);
                return url;
            }
            url = url.Remove(index,index1 - index);
            url = url.Insert(index, value);
            return url;
        }

        #region ����URL��������
        public static void GetDomain(string fromUrl, out string domain, out string subDomain)
        {
            domain = "";
            subDomain = "";
            try
            {
                if (fromUrl.IndexOf("����Ƭ") > -1)
                {
                    subDomain = fromUrl;
                    domain = "��Ƭ";
                    return;
                }

                UriBuilder builder = new UriBuilder(fromUrl);
                fromUrl = builder.ToString();

                Uri u = new Uri(fromUrl);

                if (u.IsWellFormedOriginalString())
                {
                    if (u.IsFile)
                    {
                        subDomain = domain = "�ͻ��˱����ļ�·��";

                    }
                    else
                    {
                        string Authority = u.Authority;
                        string[] ss = u.Authority.Split('.');
                        if (ss.Length == 2)
                        {
                            Authority = "www." + Authority;
                        }
                        int index = Authority.IndexOf('.', 0);
                        domain = Authority.Substring(index + 1, Authority.Length - index - 1).Replace("comhttp","com");
                        subDomain = Authority.Replace("comhttp", "com"); 
                        if (ss.Length < 2)
                        {
                            domain = "����·��";
                            subDomain = "����·��";
                        }
                    }
                }
                else
                {
                    if (u.IsFile)
                    {
                        subDomain = domain = "�ͻ��˱����ļ�·��";
                    }
                    else
                    {
                        subDomain = domain = "����·��";
                    }
                }
            }
            catch
            {
                subDomain = domain = "����·��";
            }
        }

        /// <summary>
        /// ���� url �ַ����еĲ�����Ϣ
        /// </summary>
        /// <param name="url">����� URL</param>
        /// <param name="baseUrl">��� URL �Ļ�������</param>
        /// <param name="nvc">���������õ��� (������,����ֵ) �ļ���</param>
        public static void ParseUrl(string url, out string baseUrl, out NameValueCollection nvc)
        {
            if (url == null)
                throw new ArgumentNullException("url");

            nvc = new NameValueCollection();
            baseUrl = "";

            if (url == "")
                return;

            int questionMarkIndex = url.IndexOf('?');

            if (questionMarkIndex == -1)
            {
                baseUrl = url;
                return;
            }
            baseUrl = url.Substring(0, questionMarkIndex);
            if (questionMarkIndex == url.Length - 1)
                return;
            string ps = url.Substring(questionMarkIndex + 1);

            // ��ʼ����������    
            Regex re = new Regex(@"(^|&)?(\w+)=([^&]+)(&|$)?", RegexOptions.Compiled);
            MatchCollection mc = re.Matches(ps);

            foreach (Match m in mc)
            {
                nvc.Add(m.Result("$2").ToLower(), m.Result("$3"));
            }
        }

        #endregion

        ///   <summary> 
        ///  ����.
        ///   </summary> 
        public void Test()
        {
            string pageURL = " http://www.google.com.hk/search?hl=zh-CN&source=hp&q=%E5%8D%9A%E6%B1%87%E6%95%B0%E7%A0%81&aq=f&aqi=g2&aql=&oq=&gs_rfai= ";
            Uri uri = new Uri(pageURL);
            string queryString = uri.Query;
            NameValueCollection col = GetQueryString(queryString);
            string searchKey = col["q"];
            // ��� searchKey = "��������" 
        }

        ///   <summary> 
        ///  ����ѯ�ַ�������ת��Ϊ��ֵ����.
        ///   </summary> 
        ///   <param name="queryString"></param> 
        ///   <returns></returns> 
        public static NameValueCollection GetQueryString(string queryString)
        {
            return GetQueryString(queryString, null, true);
        }

        ///   <summary> 
        ///  ����ѯ�ַ�������ת��Ϊ��ֵ����.
        ///   </summary> 
        ///   <param name="queryString"></param> 
        ///   <param name="encoding"></param> 
        ///   <param name="isEncoded"></param> 
        ///   <returns></returns> 
        public static NameValueCollection GetQueryString(string queryString, Encoding encoding, bool isEncoded)
        {
            queryString = queryString.Replace("?", "");
            NameValueCollection result = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(queryString))
            {
                int count = queryString.Length;
                for (int i = 0; i < count; i++)
                {
                    int startIndex = i;
                    int index = -1;
                    while (i < count)
                    {
                        char item = queryString[i];
                        if (item == '=')
                        {
                            if (index < 0)
                            {
                                index = i;
                            }
                        }
                        else if (item == '&')
                        {
                            break;
                        }
                        i++;
                    }
                    string key = null;
                    string value = null;
                    if (index >= 0)
                    {
                        key = queryString.Substring(startIndex, index - startIndex);
                        value = queryString.Substring(index + 1, (i - index) - 1);
                    }
                    else
                    {
                        key = queryString.Substring(startIndex, i - startIndex);
                    }
                    if (isEncoded)
                    {
                        result[MyUrlDeCode(key, encoding)] = MyUrlDeCode(value, encoding);
                    }
                    else
                    {
                        result[key] = value;
                    }
                    if ((i == (count - 1)) && (queryString[i] == '&'))
                    {
                        result[key] = string.Empty;
                    }
                }
            }
            return result;
        }

        ///   <summary> 
        ///  ����URL.
        ///   </summary> 
        ///   <param name="encoding"> nullΪ�Զ�ѡ����� </param> 
        ///   <param name="str"></param> 
        ///   <returns></returns> 
        public static string MyUrlDeCode(string str, Encoding encoding)
        {
            if (encoding == null)
            {
                Encoding utf8 = Encoding.UTF8;
                // ������utf-8���н���                      
                string code = HttpUtility.UrlDecode(str.ToUpper(), utf8);
                // ���Ѿ�������ַ��ٴν��б���. 
                string encode = HttpUtility.UrlEncode(code, utf8).ToUpper();
                if (str == encode)
                    encoding = Encoding.UTF8;
                else
                    encoding = Encoding.GetEncoding("gb2312");
            }
            return HttpUtility.UrlDecode(str, encoding);
        }
    }
}
