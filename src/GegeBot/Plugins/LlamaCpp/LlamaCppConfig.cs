namespace GegeBot.Plugins.LlamaCpp
{
    [Config("LlamaCpp")]
    internal class LlamaCppConfig
    {
        public static string ServerAddress { get; set; }
        public static bool WriteMessageLog { get; set; } = false;
        public static string BotName { get; set; }
        public static string Prompt { get; set; }
        public static decimal Temperature { get; set; } = 0.8m;
        public static int MemoryLimit { get; set; } = 30;
        public static string ResetCommand { get; set; }
        public static string ResetMessage { get; set; }
        public static List<string> FilterText { get; set; } = new List<string>();
        public static List<string> DictFile { get; set; } = new List<string>();
        public static double ReplyProbability { get; set; }
    }
}
