using System.IO;
using System.Windows;
using MediaDownloader.Common.Enum;
using MediaDownloader.Common.Module;

namespace MediaDownloader.Page.Navigation
{
    /// <summary>
    /// SettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsPage
    {
        public SettingsPage()
        {
            InitializeComponent();

            VideoSavePathTextBox.Text = ModBase.GetConfig().VideoSavePath;
            CacheSavePathTextBox.Text = ModBase.GetConfig().CacheSavePath;
            FfmpegPathTextBox.Text = ModBase.GetConfig().FfmpegPath;
        }

        private void VideoSavePathTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var path = VideoSavePathTextBox.Text;
            // check if invalid
            if (Directory.Exists(path))
            {
                ModBase.GetConfig().VideoSavePath = VideoSavePathTextBox.Text;
                ModBase.SaveConfig();
            }
            else
            {
                // fallback
                VideoSavePathTextBox.Text = ModBase.GetConfig().VideoSavePath;
                ModBase.ShowHint?.Invoke("文件夹不存在", HintLevelEnum.Warning);
            }
        }

        private void CacheSavePathTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var path = CacheSavePathTextBox.Text;
            // check if invalid
            if (Directory.Exists(path))
            {
                ModBase.GetConfig().CacheSavePath = CacheSavePathTextBox.Text;
                ModBase.SaveConfig();
            }
            else
            {
                // fallback
                CacheSavePathTextBox.Text = ModBase.GetConfig().CacheSavePath;
                ModBase.ShowHint?.Invoke("文件夹不存在", HintLevelEnum.Warning);
            }
        }

        private void FfmpegPathTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var path = FfmpegPathTextBox.Text;
            // check if invalid
            if (File.Exists(path))
            {
                ModBase.GetConfig().FfmpegPath = FfmpegPathTextBox.Text;
                ModBase.SaveConfig();
            }
            else
            {
                // fallback
                FfmpegPathTextBox.Text = ModBase.GetConfig().FfmpegPath;
                ModBase.ShowHint?.Invoke("文件不存在", HintLevelEnum.Warning);
            }
        }
    }
}
