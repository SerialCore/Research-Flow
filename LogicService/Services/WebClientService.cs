using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Cache;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace LogicService.Services
{  
    public class UploadEventArgs : EventArgs
    {
        private int bytesSent;
        private int totalBytes;

        public int BytesSent
        {
            get { return bytesSent; }
            set { bytesSent = value; }
        }
        
        public int TotalBytes
        {
            get { return totalBytes; }
            set { totalBytes = value; }
        }
    }

    public class DownloadEventArgs : EventArgs
    {
        private int bytesReceived;
        private int totalBytes;
        private byte[] receivedData;

        public int BytesReceived
        {
            get { return bytesReceived; }
            set { bytesReceived = value; }
        }
 
        public int TotalBytes
        {
            get { return totalBytes; }
            set { totalBytes = value; }
        }
 
        public byte[] ReceivedData
        {
            get { return receivedData; }
            set { receivedData = value; }
        }
    }

    public class WebClientService
    {
        private Encoding encoding = Encoding.Default;
        private string respHtml = "";
        private WebProxy proxy;
        private static CookieContainer cc;
        private WebHeaderCollection requestHeaders;
        private WebHeaderCollection responseHeaders;
        private int bufferSize = 15240;

        public event EventHandler<UploadEventArgs> UploadProgressChanged;
        public event EventHandler<DownloadEventArgs> DownloadProgressChanged;

        static WebClientService()
        {
            LoadCookiesFromDisk();
        }

        public WebClientService()
        {
            requestHeaders = new WebHeaderCollection();
            responseHeaders = new WebHeaderCollection();
        }
        
        public int BufferSize
        {
            get { return bufferSize; }
            set { bufferSize = value; }
        }

        public WebHeaderCollection ResponseHeaders
        {
            get { return responseHeaders; }
        }

        public WebHeaderCollection RequestHeaders
        {
            get { return requestHeaders; }
        }

        public WebProxy Proxy
        {
            get { return proxy; }
            set { proxy = value; }
        }

        public Encoding Encoding
        {
            get { return encoding; }
            set { encoding = value; }
        }

        public string RespHtml
        {
            get { return respHtml; }
            set { respHtml = value; }
        }

        public CookieContainer CookieContainer
        {
            get { return cc; }
            set { cc = value; }
        }

        public string GetHtml(string url)
        {
            HttpWebRequest request = CreateRequest(url, "GET");
            respHtml = encoding.GetString(GetData(request));
            return respHtml;
        }
 
        public void DownloadFile(string url, string filename)
        {
            FileStream fs = null;
            try
            {
                HttpWebRequest request = CreateRequest(url, "GET");
                byte[] data = GetData(request);
                fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
                fs.Write(data, 0, data.Length);
            }
            finally
            {
                if (fs != null) fs.Close();
            }
        }

        public byte[] GetData(string url)
        {
            HttpWebRequest request = CreateRequest(url, "GET");
            return GetData(request);
        }

        /// <summary>
        /// Post text data to a specific url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData">url encode</param>
        /// <returns></returns>
        public string Post(string url, string postData)
        {
            byte[] data = encoding.GetBytes(postData);
            return Post(url, data);
        }

        /// <summary>
        /// Post byte data to a specific url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData">url encode</param>
        /// <returns></returns>
        public string Post(string url, byte[] postData)
        {
            HttpWebRequest request = CreateRequest(url, "POST");
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postData.Length;
            request.KeepAlive = true;
            PostData(request, postData);
            respHtml = encoding.GetString(GetData(request));
            return respHtml;
        }

        /// <summary>    
        /// Post mulitpart form data to a specific url
        /// </summary>    
        /// <param name="url"></param>    
        /// <param name="mulitpartForm">mulitpart form data</param>    
        /// <returns></returns>    
        public string Post(string url, MultipartForm mulitpartForm)
        {
            HttpWebRequest request = CreateRequest(url, "POST");
            request.ContentType = mulitpartForm.ContentType;
            request.ContentLength = mulitpartForm.FormData.Length;
            request.KeepAlive = true;
            PostData(request, mulitpartForm.FormData);
            respHtml = encoding.GetString(GetData(request));
            return respHtml;
        }

        private byte[] GetData(HttpWebRequest request)
        {
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            responseHeaders = response.Headers;
            //SaveCookiesToDisk();

            DownloadEventArgs args = new DownloadEventArgs();
            if (responseHeaders[HttpResponseHeader.ContentLength] != null)
                args.TotalBytes = Convert.ToInt32(responseHeaders[HttpResponseHeader.ContentLength]);

            MemoryStream ms = new MemoryStream();
            int count = 0;
            byte[] buf = new byte[bufferSize];
            while ((count = stream.Read(buf, 0, buf.Length)) > 0)
            {
                ms.Write(buf, 0, count);
                if (this.DownloadProgressChanged != null)
                {
                    args.BytesReceived += count;
                    args.ReceivedData = new byte[count];
                    Array.Copy(buf, args.ReceivedData, count);
                    this.DownloadProgressChanged(this, args);
                }
            }
            stream.Close();
            // compress
            if (ResponseHeaders[HttpResponseHeader.ContentEncoding] != null)
            {
                MemoryStream msTemp = new MemoryStream();
                count = 0;
                buf = new byte[100];
                switch (ResponseHeaders[HttpResponseHeader.ContentEncoding].ToLower())
                {
                    case "gzip":
                        GZipStream gzip = new GZipStream(ms, CompressionMode.Decompress);
                        while ((count = gzip.Read(buf, 0, buf.Length)) > 0)
                        {
                            msTemp.Write(buf, 0, count);
                        }
                        return msTemp.ToArray();
                    case "deflate":
                        DeflateStream deflate = new DeflateStream(ms, CompressionMode.Decompress);
                        while ((count = deflate.Read(buf, 0, buf.Length)) > 0)
                        {
                            msTemp.Write(buf, 0, count);
                        }
                        return msTemp.ToArray();
                    default:
                        break;
                }
            }
            return ms.ToArray();
        }

        private void PostData(HttpWebRequest request, byte[] postData)
        {
            int offset = 0;
            int sendBufferSize = bufferSize;
            int remainBytes = 0;
            Stream stream = request.GetRequestStream();
            UploadEventArgs args = new UploadEventArgs();
            args.TotalBytes = postData.Length;
            while ((remainBytes = postData.Length - offset) > 0)
            {
                if (sendBufferSize > remainBytes) sendBufferSize = remainBytes;
                stream.Write(postData, offset, sendBufferSize);
                offset += sendBufferSize;
                if (this.UploadProgressChanged != null)
                {
                    args.BytesSent = offset;
                    this.UploadProgressChanged(this, args);
                }
            }
            stream.Close();
        }
        
        private HttpWebRequest CreateRequest(string url, string method)
        {
            Uri uri = new Uri(url);

            if (uri.Scheme == "https")
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(this.CheckValidationResult);

            // Set a default policy level for the "http:" and "https" schemes.    
            HttpRequestCachePolicy policy = new HttpRequestCachePolicy(HttpRequestCacheLevel.Revalidate);
            HttpWebRequest.DefaultCachePolicy = policy;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AllowAutoRedirect = false;
            request.AllowWriteStreamBuffering = false;
            request.Method = method;
            if (proxy != null)
                request.Proxy = proxy;
            request.CookieContainer = cc;
            foreach (string key in requestHeaders.Keys)
            {
                request.Headers.Add(key, requestHeaders[key]);
            }
            requestHeaders.Clear();
            return request;
        }

        private bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
 
        private static void SaveCookiesToDisk()
        {
            string cookieFile = System.Environment.GetFolderPath(Environment.SpecialFolder.Cookies) + "\\webclient.cookie";
            FileStream fs = null;
            try
            {
                fs = new FileStream(cookieFile, FileMode.Create);
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formater = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formater.Serialize(fs, cc);
            }
            finally
            {
                if (fs != null) fs.Close();
            }
        }
  
        private static void LoadCookiesFromDisk()
        {
            cc = new CookieContainer();
            string cookieFile = System.Environment.GetFolderPath(Environment.SpecialFolder.Cookies) + "\\webclient.cookie";
            if (!File.Exists(cookieFile))
                return;
            FileStream fs = null;
            try
            {
                fs = new FileStream(cookieFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formater = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                cc = (CookieContainer)formater.Deserialize(fs);
            }
            finally
            {
                if (fs != null) fs.Close();
            }
        }
    }

    public class MultipartForm
    {
        private Encoding encoding;
        private MemoryStream ms;
        private string boundary;
        private byte[] formData;

        public byte[] FormData
        {
            get
            {
                if (formData == null)
                {
                    byte[] buffer = encoding.GetBytes("--" + this.boundary + "--\r\n");
                    ms.Write(buffer, 0, buffer.Length);
                    formData = ms.ToArray();
                }
                return formData;
            }
        }

        public string ContentType
        {
            get { return string.Format("multipart/form-data; boundary={0}", this.boundary); }
        }

        public Encoding StringEncoding
        {
            set { encoding = value; }
            get { return encoding; }
        }

        public MultipartForm()
        {
            boundary = string.Format("--{0}--", Guid.NewGuid());
            ms = new MemoryStream();
            encoding = Encoding.Default;
        }
 
        public void AddString(string name, string value)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("--{0}\r\n", this.boundary);
            sb.AppendFormat("Content-Disposition: form-data; name=\"{0}\"\r\n", name);
            sb.Append("\r\n");
            sb.AppendFormat("{0}\r\n", value);
            byte[] buf = encoding.GetBytes(sb.ToString());
            ms.Write(buf, 0, buf.Length);
        }
    }
}
