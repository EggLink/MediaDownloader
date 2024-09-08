using System.Drawing;
using MediaDownloader.Common.Enum;
using Newtonsoft.Json;

namespace MediaDownloader.Common.Module;

public static class ModBase
{
    private static Dictionary<string, string>? _bilibiliCookieDictionary;
    private static Configuration? _config;
    public static Dictionary<string, string> GetCookies(PlatformEnum platform = PlatformEnum.Bilibili)
    {
        switch (platform)
        {
            case PlatformEnum.Bilibili:
                if (_bilibiliCookieDictionary != null)
                {
                    return _bilibiliCookieDictionary;
                }

                if (!File.Exists("cookies.json"))
                {
                    return [];
                }

                var json = File.ReadAllText("cookies.json");
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? [];
            case PlatformEnum.NeteaseMusic:
            default:
                return [];
        }
    }

    public static Thread RunInNewThread(Action action)
    {
        var thread = new Thread(() => action());
        thread.Start();
        return thread;
    }

    public delegate void ShowHintDelegate(string message, HintLevelEnum level = HintLevelEnum.Success);
    public static ShowHintDelegate? ShowHint;

    public static void SetCookies(string dedeUserID, string dedeUserIDCkMd5, string expires, string sessData, string biliJct, string refreshToken)
    {
        var dict = new Dictionary<string, string>
        {
            { "DedeUserID", dedeUserID },
            { "DedeUserID__ckMd5", dedeUserIDCkMd5 },
            { "Expires", expires },
            { "SESSDATA", sessData },
            { "bili_jct", biliJct },
            { "refresh_token", refreshToken }
        };

        // Save cookies
        var json = JsonConvert.SerializeObject(dict, Formatting.Indented);
        File.WriteAllText("cookies.json", json);

        _bilibiliCookieDictionary = dict;
    }

    public static DateTime UnixTimeStampToDateTime(int unixTimeStamp)
    {
        var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        return dt.AddSeconds(unixTimeStamp);
    }

    public static DateTime UnixTimeStampMsToDateTime(long unixTimeStamp)
    {
        var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        return dt.AddMilliseconds(unixTimeStamp);
    }

    public static Configuration GetConfig()
    {
        if (_config != null) return _config;

        var file = new FileInfo("config.json");
        if (file.Exists)
        {
            var text = File.ReadAllText(file.FullName);
            var config = JsonConvert.DeserializeObject<Configuration>(text)!;
            _config = config;
        }
        else
        {
            _config = new Configuration();
            var text = JsonConvert.SerializeObject(_config, Formatting.Indented);
            File.WriteAllText(file.FullName, text);
        }

        if (!Directory.Exists(_config.VideoSavePath))
            Directory.CreateDirectory(_config.VideoSavePath);

        if (!Directory.Exists(_config.MusicSavePath))
            Directory.CreateDirectory(_config.MusicSavePath);

        if (!Directory.Exists(_config.CacheSavePath))
            Directory.CreateDirectory(_config.CacheSavePath);

        return _config;
    }

    public static void SaveConfig()
    {
        if (_config == null) return;

        var file = new FileInfo("config.json");
        if (!file.Exists) return;
        var text = JsonConvert.SerializeObject(_config, Formatting.Indented);
        File.WriteAllText(file.FullName, text);
    }

}