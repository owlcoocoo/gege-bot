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
            CQCode.GetText("0+1,123", out _);
            CQCode.GetText("[CQ:text,text=0+1,123]0+1,123", out _);

            CQCode.GetText("[CQ:at,qq=3333333]0+1,123", out var atQQ);
            Assert.IsTrue(atQQ.Count > 0);
        }
    }
}