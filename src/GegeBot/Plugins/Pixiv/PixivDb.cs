using GegeBot.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GegeBot.Plugins.Pixiv
{
    internal class PixivDb
    {
        static IDb _db;
        public static IDb Db
        {
            get
            {
                _db ??= DbProvider.GetDb("pixiv");
                return _db;
            }
        }
    }
}
