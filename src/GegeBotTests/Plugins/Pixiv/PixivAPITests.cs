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
    public class PixivAPITests
    {
        [TestMethod()]
        public void PuzzleTest()
        {
            List<byte[]> dataList = new List<byte[]>();
            List<string> ids = new List<string>();
            for (int i = 1; i <= 4; i++)
            {
                var data = File.ReadAllBytes($"Test/{i}.jpg");
                dataList.Add(data);
                ids.Add($"id{i}");
            }

            var result = PixivAPI.Puzzle(dataList, ids, 360, 360, 90, 4);

            File.WriteAllBytes("Test/output.jpg", result);
        }
    }
}