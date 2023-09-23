using CQHttp;
using CQHttp.DTOs;
using System.Text;

namespace GegeBot.Plugins.LlamaCpp
{
    [Plugin]
    internal class LlamaCppHandler
    {
        readonly CQBot cqBot;
        readonly LlamaCppAPI llamaCppAPI = new LlamaCppAPI(LlamaCppConfig.ServerAddress);
        readonly Log log = new Log("llama_cpp");


        public LlamaCppHandler(CQBot bot)
        {
            cqBot = bot;
            cqBot.ReceivedMessage += CqBot_ReceivedMessage;
        }

        private string GetDbValue(CQEventMessageEx msg, out string key)
        {
            key = $"{BotSession.GetSessionKey(msg)}";
            return LlamaCppDb.Db.GetValue(key);
        }

        private void SaveDbValue(string key, string text)
        {
            LlamaCppDb.Db.SetValue(key, text);
        }

        private string GetPromptFromModel(LlamaCppModel model)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in model.Chats)
            {
                sb.Append($"\n{item.UserName}:{item.UserContent}");
                sb.Append($"\n{item.BotName}:{item.BotContent}");
            }

            return sb.ToString();
        }

        private void CqBot_ReceivedMessage(CQEventMessageEx obj)
        {
            string text = CQCode.GetText(obj.message, out var atList).TrimStart();
            if (obj.message_type == CQMessageType.Group
                && (!atList.Any() || !atList.Contains(obj.self_id.ToString())))
                return;

            if (LlamaCppConfig.FilterText.Contains(text)) return;

            string value = GetDbValue(obj, out string key);

            if (!string.IsNullOrEmpty(LlamaCppConfig.ResetCommand) && text == LlamaCppConfig.ResetCommand)
            {
                SaveDbValue(key, "");
                if (!string.IsNullOrEmpty(LlamaCppConfig.ResetMessage))
                    cqBot.Message_QuickReply(obj, new CQCode().SetReply(obj.message_id).SetText(LlamaCppConfig.ResetMessage));
                return;
            }

            string botName = LlamaCppConfig.BotName;
            string userName = obj.sender.nickname;
            string prompt = LlamaCppConfig.Prompt;
            LlamaCppModel model = null;

            if (!string.IsNullOrEmpty(value))
                model = Json.FromJsonString<LlamaCppModel>(value);
            if (model != null)
            {
                prompt += GetPromptFromModel(model);
            }
            else
            {
                model = new LlamaCppModel();
            }

            prompt += $"\n{userName}:{text}";
            prompt += $"\n{botName}:";

            if (!model.Stop.Any())
                model.Stop.Add("</s>");
            if (!model.Stop.Contains(botName))
                model.Stop.Add(botName);
            if (!model.Stop.Contains(userName))
                model.Stop.Add(userName);
            string content = llamaCppAPI.Completion(prompt, LlamaCppConfig.Temperature, model.Stop);

            LlamaCppChatModel chatModel = new LlamaCppChatModel
            {
                BotName = botName,
                UserName = userName,
                UserContent = text,
                BotContent = content
            };
            if (model.Chats.Count >= LlamaCppConfig.MemoryLimit)
            {
                model.Chats.RemoveAt(0);
            }
            model.Chats.Add(chatModel);
            SaveDbValue(key, Json.ToJsonString(model));

            if (LlamaCppConfig.WriteMessageLog)
                log.WriteInfo($"\n{userName}:{text}\n{botName}:{content}");

            if (string.IsNullOrWhiteSpace(content))
                content += $"\n检测到Bot无法回复，如持续出现此问题，请对我说“{LlamaCppConfig.ResetCommand}”。";

            CQCode cqCode = new CQCode();
            cqCode.SetReply(obj.message_id);
            cqCode.SetText(content);
            cqBot.Message_QuickReply(obj, cqCode);
        }
    }
}
