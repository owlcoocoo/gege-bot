using Microsoft.VisualStudio.TestTools.UnitTesting;
using GegeBot.Plugins.Pixiv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GegeBot.Plugins.Pixiv.Tests
{
    [TestClass()]
    public class PixivisionAPITests
    {
        PixivisionAPI api = new PixivisionAPI();

        [TestMethod()]
        public void GetIllustrationTest()
        {
            var result = api.GetIllustrationArticleList();
            Assert.IsTrue(result.Any());

            result = api.GetIllustrationArticleList(1, DateTime.Parse("2023-10-25"));
            Assert.IsTrue(result.Any());
        }

        [TestMethod()]
        public void GetIllustrationArticleTest()
        {
            var result = api.GetIllustrationArticle("https://www.pixivision.net/zh/a/8963");
            Assert.IsTrue(result.Title == "头发上的花样 - 发卡插画特辑 -");
            Assert.IsTrue(result.Images.Any());

            result = api.GetIllustrationArticle("https://www.pixivision.net/zh/a/8830");
            Assert.IsTrue(result.Title == "向宇宙展开的未知世界 - 《崩坏：星穹铁道》同人作品插画特辑② -");
            Assert.IsTrue(result.Images.Any());

            result = api.GetIllustrationArticle("https://www.pixivision.net/zh/a/9195");
            Assert.IsNull(result);
        }
    }
}