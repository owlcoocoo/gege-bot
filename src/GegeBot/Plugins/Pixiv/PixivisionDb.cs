using GegeBot.Db;

namespace GegeBot.Plugins.Pixiv
{
    internal class PixivisionDb
    {
        static IDb _db;
        public static IDb Db
        {
            get
            {
                _db ??= DbProvider.GetDb("pixivision");
                return _db;
            }
        }
    }
}
