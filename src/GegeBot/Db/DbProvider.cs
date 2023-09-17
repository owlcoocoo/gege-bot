namespace GegeBot.Db
{
    public class DbProvider
    {
        public static IDb GetDb(string tableName)
        {
            Directory.CreateDirectory("data");
            return new Sqlite("data/store.db", tableName);
        }
    }
}
