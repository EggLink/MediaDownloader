namespace MediaDownloader.Common;

public class ProgramConstants
{
    public static readonly Dictionary<int, string> BilibiliVideoQualityDictionary = new()
    {
        { 127, "8K 超高清" },
        { 126, "杜比视界" },
        { 125, "HDR10" },
        { 120, "4K 超清" },
        { 116, "1080P60" },
        { 112, "1080P+" },
        { 80, "1080P" },
        { 74, "720P60" },
        { 64, "720P" },
        { 32, "480P" },
        { 16, "360P" },
        { 6, "240P" }
    };
}