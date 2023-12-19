
namespace GegeBot.Plugins.EdgeGPT
{
    [Config("EdgeGpt")]
    internal class EdgeGptConfig
    {
        public static bool Enable { get; set; }
        public static string ServerAddress { get; set; }
        public static string Proxy { get; set; }
        public static string GenerateImageProxy { get; set; }
        public static bool WriteMessageLog { get; set; }
        public static string ImagesAuthCookie { get; set; }
        public static string CookieFilePath { get; set; }
        public static string PromptFilePath { get; set; }
        public static string ResetCommand { get; set; }
        public static string ResetMessage { get; set; }
        public static List<string> FilterText { get; set; } = new List<string>();
        public static List<string> DictFile { get; set; } = new List<string>();
        public static double ReplyProbability { get; set; }
    }
}
