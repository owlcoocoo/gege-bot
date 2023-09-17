namespace GegeBot.Db
{
    public interface IDb
    {
        string GetValue(string key);
        bool SetValue(string key, string value);
        bool DeleteKey(string key);
    }
}
