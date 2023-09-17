namespace GegeBot.Db
{
    public class DbProvider
    {
        public static IDb GetDb(string tableName)
        {
            return new Sqlite("data\\store.db", tableName);
        }
    }
}
