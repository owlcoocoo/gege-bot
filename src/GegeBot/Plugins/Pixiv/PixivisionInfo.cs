namespace GegeBot.Plugins.Pixiv
{
    public class PixivisionArticleCard
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Thumbnail { get; set; }
        public List<string> TagList { get; set; } = new List<string>();
        public string Date { get; set; }
        public string Url { get; set; }
    }

    public class PixivisionArticle
    {
        public string Title { get; set; }
        public string Thumbnail { get; set; }
        public string Text { get; set; }
        public List<PixivisionArticleImage> Images { get; set; } = new List<PixivisionArticleImage>();
    }

    public class PixivisionArticleImage
    {
        public string ImageId { get; set; }
        public string ImageTitle { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string ImageUrl { get; set; }
    }
}
