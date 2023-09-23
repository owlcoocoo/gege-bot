using GegeBot.Db;

namespace GegeBot.Plugins.LlamaCpp
{
    internal class LlamaCppDb
    {
        static IDb _db;
        public static IDb Db
        {
            get
            {
                _db ??= DbProvider.GetDb("llama_cpp");
                return _db;
            }
        }
    }
}
