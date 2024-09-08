using System.ComponentModel;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Reflection;
using Downloader;
using DownloadProgressChangedEventArgs = Downloader.DownloadProgressChangedEventArgs;

namespace MediaDownloader.Common.Module;

public static class ModNetwork
{
    public static async ValueTask<JObject> SendGetRequestAsync(string url, Dictionary<string, object> param, Dictionary<string, string>? cookies = null, string referer = "https://www.bilibili.com")
    {
        var client = new HttpClient();
        // Add parameters to the URL
        if (param.Count > 0)
        {
            var paramStr = string.Join("&", param.Select(x => $"{x.Key}={x.Value}"));
            url += $"?{paramStr}";
        }
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
        if (cookies != null)
        {
            foreach (var (key, value) in cookies)
            {
                client.DefaultRequestHeaders.Add("Cookie", $"{key}={value}");
            }
        }

        // Add referer
        client.DefaultRequestHeaders.Add("Referer", referer);

        var response = await client.GetAsync(url);
        var result = await response.Content.ReadAsStringAsync();
        return JObject.Parse(result);
    }

    public static async ValueTask<JObject> SendPostRequestAsync(string url, Dictionary<string, string> param, Dictionary<string, string>? cookies = null)
    {
        var client = new HttpClient();
        var content = new FormUrlEncodedContent(param);

        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
        if (cookies != null)
        {
            foreach (var (key, value) in cookies)
            {
                client.DefaultRequestHeaders.Add("Cookie", $"{key}={value}");
            }
        }

        var response = await client.PostAsync(url, content);
        var result = await response.Content.ReadAsStringAsync();
        return JObject.Parse(result);
    }

    public static async ValueTask DownloadFileAsync(string url, string saveFilePath,
        Dictionary<string, string>? cookies = null,
        string referer = "https://www.bilibili.com", EventHandler<DownloadStartedEventArgs>? downloadStart = null, EventHandler<AsyncCompletedEventArgs>? downloadEnd = null,
        EventHandler<DownloadProgressChangedEventArgs>? downloadProgress = null)
    {
        var downloadOpt = new DownloadConfiguration()
        {
            BufferBlockSize = 10240,
            ChunkCount = 4,
            MaxTryAgainOnFailover = 5,
            ParallelDownload = true,
            Timeout = 1000,
            RequestConfiguration =
            {
                Accept = "*/*",
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                Headers = [],
                KeepAlive = false,
                ProtocolVersion = HttpVersion.Version11, // Default value is HTTP 1.1
                UseDefaultCredentials = false,
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3",
                Referer = referer
            }
        };

        if (cookies != null)
        {
            foreach (var (key, value) in cookies)
            {
                downloadOpt.RequestConfiguration.Headers.Add("Cookie", $"{key}={value}");
            }
        }

        var downloader = new DownloadService(downloadOpt);

        if (downloadStart != null)
        {
            downloader.DownloadStarted += downloadStart;
        }

        if (downloadEnd != null)
        {
            downloader.DownloadFileCompleted += downloadEnd;
        }

        if (downloadProgress != null)
        {
            downloader.DownloadProgressChanged += downloadProgress;
        }

        // start downloading
        await downloader.DownloadFileTaskAsync(url, saveFilePath);
    }
}