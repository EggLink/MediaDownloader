using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Downloader;
using MediaDownloader.Common;
using MediaDownloader.Common.Enum;
using MediaDownloader.Common.Module;
using Newtonsoft.Json.Linq;

namespace MediaDownloader.Page.Navigation;

/// <summary>
/// BilibiliPage.xaml 的交互逻辑
/// </summary>
public partial class BilibiliPage
{
    private bool _isVip;
    private int _downloadCount;

    public BilibiliPage()
    {
        InitializeComponent();
        _ = CheckLoginStatus();
    }

    public async ValueTask CheckLoginStatus()
    {
        var ck = ModBase.GetCookies();
        var resp = await ModNetwork.SendGetRequestAsync("https://api.bilibili.com/x/web-interface/nav", [], ck);
        if (resp["code"]?.ToString() == "0")
        {
            StatusCardTitleText.Text = "已登录：" + resp["data"]?["uname"];
            string hint;
            if (resp["data"]?["vipStatus"]?.ToObject<bool>() == true)
            {
                hint = "你的大会员将在 " + ModBase
                    .UnixTimeStampMsToDateTime(resp["data"]?["vipDueDate"]?.ToObject<long>() ?? 0)
                    .ToString("yyyy-M-d") + " 到期";

                _isVip = true;
            }
            else
                hint = "你还不是大会员";

            StatusCardHintText.Text = hint;
        }
    }

    private async void ExecuteButton_Click(object sender, RoutedEventArgs e)
    {
        var vid = VideoIdTextBox.Text;
        var ck = ModBase.GetCookies();
        var dict = new Dictionary<string, object>();
        if(vid.ToLower().Contains("av"))
        {
            if (int.TryParse(vid[2..], out _))
                dict.Add("aid", int.Parse(vid[2..]));  // remove "av"
            else
            {
                ModBase.ShowHint?.Invoke("请输入正确的视频ID", HintLevelEnum.Error);
                return;
            }
        }
        else if (vid.ToLower().Contains("bv"))
        {
            dict.Add("bvid", vid);  // bvid
        }
        else
        {
            ModBase.ShowHint?.Invoke("请输入正确的视频ID", HintLevelEnum.Error);
            return;
        }

        VideoIdTextBox.Tag = vid;  // avoid re-parse

        VideoInfoDockPanel.Visibility = Visibility.Collapsed;
        DownloadStatusGrid.Visibility = Visibility.Collapsed;
        EpSelectComboBox.Items.Clear();
        VideoQualityComboBox.Items.Clear();

        var resp = await ModNetwork.SendGetRequestAsync("https://api.bilibili.com/x/web-interface/view", dict, ck);
        if (resp["code"]?.ToString() != "0")
        {
            ModBase.ShowHint?.Invoke("获取视频信息失败", HintLevelEnum.Error);
            return;
        }

        var title = resp["data"]?["title"]?.ToString() ?? "";
        var pages = resp["data"]?["pages"]?.ToObject<List<JObject>>() ?? [];

        foreach (var page in pages)
        {
            var add = new ComboBoxItem
            {
                Content = page["part"]?.ToString() ?? "",
                Tag = page["cid"]?.ToString() ?? ""
            };
            EpSelectComboBox.Items.Add(add);

            if (page["page"]?.ToString() == "1")
            {
                // default
                EpSelectComboBox.SelectedItem = add;
            }
        }

        VideoNameTextBlock.Text = title;

        VideoInfoDockPanel.Visibility = Visibility.Visible;
    }

    private async void EpSelectComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = (ComboBoxItem)EpSelectComboBox.SelectedItem;
        if (item == null) return;
        var cid = item.Tag.ToString() ?? "";
        var ck = ModBase.GetCookies();
        var dict = new Dictionary<string, object>
        {
            { "cid", cid }
        };

        var vid = VideoIdTextBox.Tag.ToString() ?? "";
        if (vid.ToLower().Contains("av"))
        {
            dict.Add("aid", int.Parse(vid[2..]));  // remove "av", check in ExecuteButton_Click
        }
        else
        {
            dict.Add("bvid", vid);  // bvid
        }

        dict.Add("fnval", 16 + 256 + 512 + 64 + 128 + 1024);
        dict.Add("fnver", 0);
        dict.Add("fourk", 1);

        VideoQualityComboBox.Items.Clear();

        var resp = await ModNetwork.SendGetRequestAsync("https://api.bilibili.com/x/player/playurl", dict, ck);
        if (resp["code"]?.ToString() != "0")
        {
            ModBase.ShowHint?.Invoke("获取视频信息失败", HintLevelEnum.Error);
            return;
        }

        Dictionary<int, List<string>> acceptIds = [];
        foreach (var video in (resp["data"]?["dash"]?["video"]?.ToObject<List<JObject>>() ?? []).Where(video => video["codecs"]?.ToString().Contains("avc1") != false))
        {
            if (!acceptIds.TryGetValue(video["id"]?.ToObject<int>() ?? 0, out var dic))
            {
                acceptIds.Add(video["id"]?.ToObject<int>() ?? 0, []);
                dic = acceptIds[video["id"]?.ToObject<int>() ?? 0];
            }

            dic.Add(video["base_url"]?.ToString() ?? "");
        }


        foreach (var ids in acceptIds)
        {
            ids.Value.Add((resp["data"]?["dash"]?["audio"]?.ToObject<List<JObject>>() ?? []).FirstOrDefault()?["base_url"]?.ToString() ?? "");
        }

        foreach (var acceptId in acceptIds)
        {
            VideoQualityComboBox.Items.Add(new ComboBoxItem
            {
                Content = ProgramConstants.BilibiliVideoQualityDictionary[acceptId.Key],
                Tag = acceptId.Value
            });
        }

        VideoQualityComboBox.SelectedIndex = 0;
    }

    private void ConfirmDownloadButton_OnClick(object sender, RoutedEventArgs e)
    {
        // get all download url
        var item = (ComboBoxItem)VideoQualityComboBox.SelectedItem;
        var downloadUrlList = item.Tag as List<string>;
        DownloadStatusGrid.Visibility = Visibility.Visible;
        var ck = ModBase.GetCookies();
        _downloadCount = 0;
        DownloadProgressBar.Value = 0;

        ModBase.RunInNewThread(() =>
        {
            List<string> files = [];
            foreach (var url in downloadUrlList!)
            {
                // create download task
                _downloadCount++;
                var file = $"{DateTime.Now:HH-mm-ss}.{downloadUrlList.IndexOf(url)}.m4s";
                files.Add(file);
                var task = ModNetwork.DownloadFileAsync(url,
                    Path.Combine(ModBase.GetConfig().CacheSavePath, file), ck, downloadProgress: DownloadProgress, downloadEnd: DownloadEnd);
                task.AsTask().Wait();
            }

            if (!File.Exists(ModBase.GetConfig().FfmpegPath))
            {
                Dispatcher.Invoke(() =>
                {
                    ModBase.ShowHint?.Invoke("ffmpeg 未找到，请检查配置", HintLevelEnum.Warning);
                });
                return;
            }

            var output = Path.Combine(ModBase.GetConfig().CacheSavePath, $"{DateTime.Now:HH-mm-ss}.mp4");
            var args = files.Aggregate("", (current, file) => current + $"-i \"{Path.Combine(ModBase.GetConfig().CacheSavePath, file)}\" ");

            args += $"-codec copy \"{output}\" -y";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ModBase.GetConfig().FfmpegPath,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            process.Start();

            process.WaitForExit();

            foreach (var file in files)
            {
                File.Delete(Path.Combine(ModBase.GetConfig().CacheSavePath, file));
            }

            Dispatcher.Invoke(() =>
            {
                ModBase.ShowHint?.Invoke("下载完成");
                Process.Start(new ProcessStartInfo
                {
                    FileName = output,
                    UseShellExecute = true
                });
            });
        });
    }

    private void DownloadEnd(object? sender, AsyncCompletedEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            var text = "下载成功";
            if (e.Error != null)
                text = "下载发生错误：" + e.Error.Message;
            DownloadStatusTextBlock.Text = text;
        });
    }

    private void DownloadProgress(object? sender, DownloadProgressChangedEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            var text =
                $"下载第{_downloadCount}个文件中： {double.Round(e.ReceivedBytesSize / 1024f / 1024f, 2)}MB/{double.Round(e.TotalBytesToReceive / 1024f / 1024f, 2)}MB，速度 {double.Round(e.BytesPerSecondSpeed / 1024f / 1024f, 2)} MB/s";
            DownloadStatusTextBlock.Text = text;
            DownloadProgressBar.Value = e.ProgressPercentage;
        });
    }
}