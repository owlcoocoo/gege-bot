using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CQHttp.DTOs.Tests
{
    [TestClass()]
    public class CQCodeTests
    {
        [TestMethod()]
        public void GetTextTest()
        {
            // 测试异常
            CQCode.GetText("0+1,123", out _, out _);
            CQCode.GetText("[CQ:text,text=0+1,123]0+1,123", out _, out _);

            CQCode.GetText("[CQ:at,qq=3333333]0+1,123", out var atQQ, out _);
            Assert.IsTrue(atQQ.Count > 0);

            var text = CQCode.GetText("[CQ:reply,id=-1595206605] [CQ:at,qq=123456] [CQ:at,qq=123456] [CQ:image,file=bc70369e88c45a314592134f2114e108.image,subType=1,url=https://gchat.qpic.cn/gchatpic_new] 你好", out atQQ, out var images);
            Assert.IsTrue(atQQ.Count > 0 && text.Trim() == "你好" && images.Count > 0);

            text = CQCode.GetText("[CQ:reply,id=-1595206605] [CQ:at,qq=123456] [CQ:at,qq=123456] [CQ:text,text=你好]", out atQQ, out _);
            Assert.IsTrue(atQQ.Count > 0 && text.Trim() == "你好");
        }
    }
}