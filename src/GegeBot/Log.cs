namespace GegeBot
{
    internal class Log
    {
        readonly string dir = "logs";

        public Log(string dir = "")
        {
            this.dir += "/" + dir;
        }

        public void WriteError(string text)
        {
            string output = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [ERROR] {text}";
            Console.WriteLine(output);
            Directory.CreateDirectory(dir);
            File.AppendAllLines($"{dir}/{DateTime.Now:yyyyMMdd}.log", new string[] { output });
        }

        public void WriteInfo(string text)
        {
            string output = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [Info] {text}";
            Directory.CreateDirectory(dir);
            File.AppendAllLines($"{dir}/{DateTime.Now:yyyyMMdd}.log", new string[] { output });
        }
    }
}
