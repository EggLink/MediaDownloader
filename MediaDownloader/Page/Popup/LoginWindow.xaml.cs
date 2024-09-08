using System.Drawing.Imaging;
using System.IO;
using System.Net.Mime;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Gma.QrCodeNet.Encoding.Windows.Render;
using Gma.QrCodeNet.Encoding;
using MediaDownloader.Common.Enum;
using MediaDownloader.Common.Module;
using System.Web;
using Wpf.Ui.Controls;

namespace MediaDownloader.Page.Popup;

/// <summary>
/// LoginWindow.xaml 的交互逻辑
/// </summary>
public partial class LoginWindow
{
    public PlatformEnum LoginPlatform { get; set; }
    public string CodeKey { get; set; } = string.Empty;

    public LoginWindow(PlatformEnum platform)
    {
        InitializeComponent();

        switch (platform)
        {
            case PlatformEnum.Bilibili:
                LoginPlatform = PlatformEnum.Bilibili;
                Title = "哔哩哔哩登录";
                WindowTitleBar.Title = Title;
                _ = InitializeBilibili();
                break;
            case PlatformEnum.NeteaseMusic:
                LoginPlatform = PlatformEnum.NeteaseMusic;
                Title = "网易云音乐登录";
                WindowTitleBar.Title = Title;
                InitializeNetease();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(platform), platform, null);
        }
    }

    public async ValueTask InitializeBilibili()
    {
        var resp = await ModNetwork.SendGetRequestAsync("https://passport.bilibili.com/x/passport-login/web/qrcode/generate", []);
        var url = resp["data"]?["url"]?.ToString();
        if (url != null)
        {
            var qrEncoder = new QrEncoder(ErrorCorrectionLevel.H);
            qrEncoder.TryEncode(url, out var qrCode);

            var renderer = new GraphicsRenderer(new FixedModuleSize(5, QuietZoneModules.Two), System.Drawing.Brushes.Black, System.Drawing.Brushes.White);
            using var stream = new MemoryStream();
            renderer.WriteToStream(qrCode.Matrix, ImageFormat.Png, stream);
            stream.Seek(0, SeekOrigin.Begin);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = stream;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            QrCodeImage.Source = bitmap;
            CodeKey = resp["data"]?["qrcode_key"]?.ToString() ?? "";
        }

        ModBase.RunInNewThread(async () =>
        {
            while (true)
            {
                var checkResp = await ModNetwork.SendGetRequestAsync(
                    "https://passport.bilibili.com/x/passport-login/web/qrcode/poll",
                    new Dictionary<string, object> { { "qrcode_key", CodeKey } });
                if (checkResp["data"]?["code"]?.ToObject<int>() == 0)  // Success
                {
                    var uri = new Uri(checkResp["data"]?["url"]?.ToString()!);
                    var query = HttpUtility.ParseQueryString(uri.Query);

                    var dedeUserID = query["DedeUserID"]!;
                    var dedeUserIDCkMd5 = query["DedeUserID__ckMd5"]!;
                    var expires = query["Expires"]!;
                    var sessData = query["SESSDATA"]!;
                    var biliJct = query["bili_jct"]!;
                    var refreshToken = checkResp["data"]?["refresh_token"]?.ToString()!;

                    ModBase.SetCookies(dedeUserID, dedeUserIDCkMd5, expires, sessData, biliJct, refreshToken);
                    Dispatcher.Invoke(() =>
                    {
                        Close();
                        ModBase.ShowHint?.Invoke("登录成功");
                    });
                    break;
                }

                if (checkResp["data"]?["code"]?.ToObject<int>() == 86038)  // Timeout
                {
                    // generate new QR code
                    await InitializeBilibili();
                    break;
                }

                if (checkResp["data"]?["code"]?.ToObject<int>() == 86090)  // Scanned
                {
                    Dispatcher.Invoke(() => CodeStatus.Text = "请确认登录");
                }

                if (checkResp["data"]?["code"]?.ToObject<int>() == 86101)  // Not Scan
                {
                    Dispatcher.Invoke(() => CodeStatus.Text = "等待扫描中");
                }

                await Task.Delay(1000);
            }
        });
    }

    public void InitializeNetease()
    {

    }
}