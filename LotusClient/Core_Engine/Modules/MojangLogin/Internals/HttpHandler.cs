using System.Collections.Specialized;
using System.Net;
using System.Web;

namespace Core_Engine.Modules.Networking.Internals
{
    public static class HttpHandler
    {
        static HttpListener? listener;
        static HttpClient? httpClient = null;

        public static void openServer(string url)
        {
            if (listener != null)
            {
                return;
            }
            listener = new HttpListener();
            listener.Prefixes.Add(url + "/");
            listener.Start();
        }

        public static void closeServer()
        {
            if (listener == null)
            {
                return;
            }
            listener.Stop();
            listener.Close();
            listener = null;
        }

        public static NameValueCollection? GetQueries()
        {
            if (listener == null)
            {
                return null;
            }
            return HttpUtility.ParseQueryString(listener.GetContext().Request.Url!.Query);
        }

        public static async Task<HttpResponseMessage> SendRequest(HttpRequestMessage msg)
        {
            if (httpClient == null)
            {
                httpClient = new HttpClient();
            }
            var response = await httpClient.SendAsync(msg);
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
    }
}
