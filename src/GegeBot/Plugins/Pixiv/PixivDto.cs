namespace GegeBot.Plugins.Pixiv
{
    public class PixivDto
    {
        public string Alt { get; set; } = "";
        public List<byte[]> Images { get; set; } = new();
        public string Message { get; set; }
        public string ImageMessage { get; set; }
        public int ImageCount { get; set; }
    }
}
