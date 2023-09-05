namespace GegeBot.Plugins.Pixiv
{
    [Config("Pixiv")]
    internal static class PixivConfig
    {
        public static string Cookie { get; set; }
        public static string Proxy { get; set; }
        public static string PximgReverseProxy { get; set; }
        public static int ImageQuality { get; set; } = 80;
        public static int MaxImages { get; set; } = 3;
        public static int Timeout { get; set; } = 60;
    }
}
