namespace GegeBot.Plugins.Pixiv
{
    public class PixivDto
    {
        public string Alt { get; set; } = "";
        public List<string> Images { get; set; } = new();
        public string Message { get; set; }
    }
}
