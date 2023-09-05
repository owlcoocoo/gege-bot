namespace GegeBot
{
    [Config("Bot")]
    internal static class BotConfig
    {
        public static string WSAddress { get; set; }
        public static long ManagerQQ { get; set; }
        public static bool IsSendErrorMessage { get; set; } = false;
        public static int DeleteErrorMessageTimeout { get; set; } = 30;
    }
}
