using System.Collections.Specialized;
using System.Net;
using System.Web;

namespace LotusCore.Utils
{
    public static class HttpHandler
    {
        static HttpListener? _Listener;
        static HttpClient? _HttpClient = null;

        public static void openServer(string url)
        {
            if (_Listener != null)
            {
                return;
            }
            _Listener = new HttpListener();
            _Listener.Prefixes.Add(url + "/");
            _Listener.Start();
        }

        public static void closeServer()
        {
            if (_Listener == null)
            {
                return;
            }
            _Listener.Stop();
            _Listener.Close();
            _Listener = null;
        }

        public static NameValueCollection? GetQueries()
        {
            if (_Listener == null)
            {
                return null;
            }
            return HttpUtility.ParseQueryString(_Listener.GetContext().Request.Url!.Query);
        }

        public static async Task<HttpResponseMessage> SendRequest(HttpRequestMessage msg)
        {
            if (_HttpClient == null)
            {
                _HttpClient = new HttpClient();
            }
            var response = await _HttpClient.SendAsync(msg);
            return response;
        }

        public static HttpRequestMessage CreateHttpRequestMessage(
            HttpMethod httpMethod,
            string Uri,
            HttpContent? httpContent
        )
        {
            HttpRequestMessage msg = new HttpRequestMessage();
            msg.Method = httpMethod;
            msg.RequestUri = new Uri(Uri);
            msg.Content = httpContent;
            return msg;
        }

        public static async Task<byte[]> DownloadSkinTexture(string url)
        {
            if (_HttpClient == null)
            {
                _HttpClient = new HttpClient();
            }
            try
            {
                return await _HttpClient.GetByteArrayAsync(url);
            }
            catch (Exception e)
            {
                Logging.LogError($"DownloadSkinTexture: {e}");
                return [];
            }
        }
    }
}
