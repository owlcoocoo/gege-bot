using CQHttp;
using CQHttp.DTOs;

namespace GegeBot.Plugins.Poke
{
    [Plugin]
    internal class PokeHandler
    {
        readonly CQBot cqBot;

        public PokeHandler(CQBot bot)
        {
            cqBot = bot;
            cqBot.ReceivedPoke += CqBot_ReceivedPoke; ;
        }

        List<string> GetFiles()
        {
            string[] extensions = [".png", ".jpg", ".jpeg", ".gif"];
            return Directory.EnumerateFiles(PokeConfig.FilePath, "*.*").Where(x => extensions.Any(a => x.ToLower().EndsWith(a))).ToList();
        }

        private void CqBot_ReceivedPoke(CQEventPoke obj)
        {
            if (obj.target_id.ToString() != cqBot.BotID) return;
            var files = GetFiles();
            if (files.Count < 1) return;
            int n = new Random().Next(files.Count - 1);
            byte[] data = File.ReadAllBytes(files[n]);
            var cqCode = new CQCode().SetImage(data, true);
            if (obj.group_id != 0)
            {
                cqBot.Message_SendGroupMsg(obj.group_id, cqCode);
            }
            else
            {
                cqBot.Message_SendPrivateMsg(obj.user_id, cqCode);
            }
        }
    }
}
