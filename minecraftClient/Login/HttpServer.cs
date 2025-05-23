using System.Net;
using System.Web;
using Silk.NET.OpenAL;
using System.Collections.Specialized;

public static class HttpServer
{
    static HttpListener? listener;
    static HttpClient? httpClient = null;

    public static void openServer()
    {
        if (listener != null)
        {
            return;
        }
        listener = new HttpListener();
        listener.Prefixes.Add(Login.redirect_uri + "/");
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
}