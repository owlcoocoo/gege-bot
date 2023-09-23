namespace GegeBot.Plugins.LlamaCpp
{
    internal class LlamaCppChatModel
    {
        public string BotName { get; set; }
        public string UserName { get; set; }
        public string UserContent { get; set; }
        public string BotContent { get; set; }
    }

    internal class LlamaCppModel
    {
        public List<string> Stop { get; set; } = new();
        public List<LlamaCppChatModel> Chats { get; set; } = new();
    }
}
