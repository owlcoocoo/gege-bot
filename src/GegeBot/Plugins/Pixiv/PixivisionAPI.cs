using HtmlAgilityPack;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GegeBot.Plugins.Pixiv
{
    public class PixivisionAPI
    {
        const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/118.0.0.0 Safari/537.36";

        const string Domain = "https://www.pixivision.net";

        readonly RestClient client;

        public PixivisionAPI()
        {
            var options = new RestClientOptions()
            {
                UserAgent = UserAgent,
            };
            if (!string.IsNullOrEmpty(PixivConfig.Proxy))
                options.Proxy = new WebProxy(PixivConfig.Proxy);
            client = new RestClient(options);
            client.AddDefaultHeader("Accept", "*/*");
            client.AddDefaultHeader("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8");
        }

        private RestResponse ExecuteAndRetry(RestRequest request, int retryCount = 3)
        {
            int retryCounter = 0;
            request.Timeout = 1000 * 20;

            RestResponse response;
            do
            {
                response = client.Execute(request);
                if (!response.IsSuccessful || response.ContentLength < 1)
                {
                    retryCounter++;
                }
                else break;
            }
            while (retryCounter < retryCount);

            return response;
        }

        private string GetSubsting(string text, string startText, string endText = "", string startText2 = "")
        {
            int startIndex = text.IndexOf(startText);
            if (!string.IsNullOrEmpty(startText2))
            {
                startIndex = text.IndexOf(startText2, startIndex);
            }
            int endIndex = string.IsNullOrEmpty(endText) ? text.Length : text.LastIndexOf(endText);
            startIndex += !string.IsNullOrEmpty(startText2) ? startText2.Length : startText.Length;
            return text.Substring(startIndex, endIndex - startIndex);
        }

        private HtmlNodeCollection SelectNodes(HtmlNode htmlNode, string xpath)
        {
            return htmlNode.SelectNodes(htmlNode.XPath + xpath);
        }

        private HtmlNode SelectSingleNode(HtmlNode htmlNode, string xpath)
        {
            return htmlNode.SelectSingleNode(htmlNode.XPath + xpath);
        }

        private List<PixivisionArticleCard> GetPixivisionArticleCard(string html)
        {
            List<PixivisionArticleCard> list = new List<PixivisionArticleCard>();

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var cardColumn = htmlDoc.DocumentNode.SelectSingleNode("//ul[contains(@class,\"main-column-container\")]");
            var cardContainerList = SelectNodes(cardColumn, "/li");

            foreach (var container in cardContainerList)
            {
                PixivisionArticleCard card = new PixivisionArticleCard();

                var thumbnailContainer = SelectSingleNode(container, "/article/div[1]");
                string thumbnail = GetSubsting(thumbnailContainer.InnerHtml, "url(", ")");
                card.Thumbnail = thumbnail;

                var titleContainer = SelectSingleNode(container, "/article/div[2]");
                string title = SelectSingleNode(titleContainer, "/h2/a").InnerText;
                string url = SelectSingleNode(titleContainer, "/h2/a").GetAttributeValue("href", "");
                card.Id = GetSubsting(url, "a/");
                card.Title = title;
                card.Url = Domain + url;

                var footerContainer = SelectSingleNode(container, "/article/div[3]");
                var tagList = SelectNodes(footerContainer, "/ul/li");
                foreach (var tagItem in tagList)
                {
                    string tag = SelectSingleNode(tagItem, "/a").GetAttributeValue("data-gtm-label", "");
                    card.TagList.Add(tag);
                }
                string date = SelectSingleNode(footerContainer, "/div/time").GetAttributeValue("datetime", "");
                card.Date = date;

                list.Add(card);
            }

            return list;
        }

        private PixivisionArticle GetPixivisionArticle(string html)
        {
            PixivisionArticle pixivisionArticle = new PixivisionArticle();

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var articleColumn = htmlDoc.DocumentNode.SelectSingleNode("//article");

            pixivisionArticle.Title = SelectSingleNode(articleColumn, "/header/h1").InnerText;
            pixivisionArticle.Thumbnail = SelectSingleNode(articleColumn, "/div[1]/div[1]/div[1]/div[1]/div/img").GetAttributeValue("src", "");
            pixivisionArticle.Text = SelectSingleNode(articleColumn, "/div[1]/div[1]/div[2]/div").InnerHtml;
            pixivisionArticle.Text = pixivisionArticle.Text.Replace("<p>", "").Replace("</p>", "\n");

            var illustsContainer = SelectNodes(articleColumn, "/div[1]/div[1]/div[contains(@class,\"_feature-article-body__pixiv_illust\")]");
            foreach (var item in illustsContainer)
            {
                PixivisionArticleImage image =  new PixivisionArticleImage();
                image.ImageTitle = SelectSingleNode(item, "/div/div[1]/div/h3/a").InnerText;
                string titleUrl = SelectSingleNode(item, "/div/div[1]/div/h3/a").GetAttributeValue("href", "");
                image.ImageId = GetSubsting(titleUrl, "artworks/", "?");
                image.UserName = SelectSingleNode(item, "/div/div[1]/div/p/a").InnerText;
                string userUrl = SelectSingleNode(item, "/div/div[1]/div/p/a").GetAttributeValue("href", "");
                image.UserId = GetSubsting(userUrl, "users/", "?");
                image.ImageUrl = SelectSingleNode(item, "/div/div[2]/a//img").GetAttributeValue("src", "");
                pixivisionArticle.Images.Add(image);
            }

            return pixivisionArticle;
        }

        /// <summary>
        /// 获取插画文章
        /// </summary>
        /// <param name="url">页面链接</param>
        /// <returns></returns>
        public PixivisionArticle GetIllustrationArticle(string url)
        {
            var request = new RestRequest(url, Method.Get);
            var response = ExecuteAndRetry(request);
            var article = GetPixivisionArticle(response.Content);
            return article;
        }

        /// <summary>
        /// 获取最新插画列表
        /// </summary>
        /// <param name="page">页数</param>
        /// <param name="date">匹配的日期，格式：yyyy-MM-dd</param>
        /// <returns></returns>
        public List<PixivisionArticleCard> GetIllustrationArticleList(int page = 1, DateTime? date = null)
        {
            var request = new RestRequest($"{Domain}/zh/c/illustration/?p={page}", Method.Get);
            var response = ExecuteAndRetry(request);
            var list = GetPixivisionArticleCard(response.Content);
            if (date != null)
            {
                list = list.Where(a => a.Date == date.Value.ToString("yyyy-MM-dd")).ToList();
            }
            return list;
        }
    }
}
