using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GegeBot.Db.Tests
{
    [TestClass()]
    public class SqliteTests
    {
        readonly Sqlite db = new Sqlite("test.db", "test");

        const string key = "test";

        [TestMethod()]
        public void SetValueTest()
        {
            db.SetValue(key, "222");
            db.SetValue(key, "333");
        }

        [TestMethod()]
        public void GetValueTest()
        {
            var result = db.GetValue(key);
            Assert.IsTrue(result == "333");
        }

        [TestMethod()]
        public void DeleteKeyTest()
        {
            var result = db.DeleteKey(key);
            Assert.IsTrue(result);
        }
    }
}