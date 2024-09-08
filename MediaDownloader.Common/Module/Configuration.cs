namespace MediaDownloader.Common.Module;

public class Configuration
{
    public string VideoSavePath { get; set; } = Path.Combine(Environment.CurrentDirectory, "DownloadVideo");
    public string MusicSavePath { get; set; } = Path.Combine(Environment.CurrentDirectory, "DownloadMusic");
    public string CacheSavePath { get; set; } = Path.Combine(Environment.CurrentDirectory, "Cache");
    public string FfmpegPath { get; set; } = "ffmpeg";
}