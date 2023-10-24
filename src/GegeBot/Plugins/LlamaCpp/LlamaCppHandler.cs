using CQHttp;
using CQHttp.DTOs;
using System.Collections;
using System.Text;

namespace GegeBot.Plugins.LlamaCpp
{
    [Plugin]
    internal class LlamaCppHandler
    {
        readonly CQBot cqBot;
        readonly LlamaCppAPI llamaCppAPI = new LlamaCppAPI(LlamaCppConfig.ServerAddress);
        readonly Log log = new Log("llama_cpp");

        readonly Hashtable dictionary = new Hashtable();

        public LlamaCppHandler(CQBot bot)
        {
            foreach (string file in LlamaCppConfig.DictFile)
            {
                Console.WriteLine($"[LlamaCpp]加载词库 {file}");

                string data = File.ReadAllText(file);
                var jsonArray = Json.ToJsonNode(data).AsArray();
                foreach (var item in jsonArray)
                {
                    dictionary.Add(item.ToString(), null);
                }
            }

            Console.WriteLine($"[LlamaCpp]词库加载完毕，共计 {dictionary.Count} 条。");

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

            foreach (string keyword in LlamaCppConfig.FilterText)
            {
                if (text.StartsWith(keyword))
                {
                    return;
                }
            }

            bool isKeyword = LlamaCppConfig.ReplyProbability > 0 && dictionary.Count > 0
                             && !string.IsNullOrWhiteSpace(text) && dictionary.ContainsKey(text);
            if (isKeyword)
            {
                double num = new Random().NextDouble();
                if (num >= LlamaCppConfig.ReplyProbability)
                    isKeyword = false;
            }

            if (!isKeyword && obj.message_type == CQMessageType.Group &&
                (!atList.Any() || !atList.Contains(obj.self_id.ToString())))
                return;

            string botName = LlamaCppConfig.BotName;
            string userName = obj.sender.nickname;
            if (LlamaCppConfig.UseGroupCard && obj.message_type == CQMessageType.Group)
            {
                CQGroupMemberInfo memberInfo = cqBot.Group_GetGroupMemberInfoSync(obj.group_id, obj.self_id);
                if (memberInfo != null && !string.IsNullOrEmpty(memberInfo.card))
                    botName = memberInfo.card;

                if (!string.IsNullOrEmpty(obj.sender.card))
                    userName = obj.sender.card;
            }

            int retryCounter = 0;

        RETRY:

            string value = GetDbValue(obj, out string key);

            if (!string.IsNullOrEmpty(LlamaCppConfig.ResetCommand) && text == LlamaCppConfig.ResetCommand)
            {
                SaveDbValue(key, "");
                if (!string.IsNullOrEmpty(LlamaCppConfig.ResetMessage))
                    cqBot.Message_QuickReply(obj, new CQCode().SetReply(obj.message_id).SetText(LlamaCppConfig.ResetMessage));
                return;
            }

            string prompt = LlamaCppConfig.Prompt;
            prompt = prompt.Replace("{{BotName}}", botName);
            prompt = prompt.Replace("{{StopText}}", LlamaCppConfig.StopText);
            var tokens = llamaCppAPI.Tokenize(prompt);

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

            if (!string.IsNullOrEmpty(LlamaCppConfig.StopText) && !model.Stop.Contains(LlamaCppConfig.StopText))
                model.Stop.Add(LlamaCppConfig.StopText);
            string name = $"{userName}:";
            if (!model.Stop.Contains(name))
                model.Stop.Add(name);

            string content = llamaCppAPI.Completion(prompt, LlamaCppConfig.Temperature, model.Stop, n_keep: tokens.Count);
            if (string.IsNullOrWhiteSpace(content))
            {
                if (retryCounter < 1)
                {
                    SaveDbValue(key, "");
                    retryCounter++;
                    goto RETRY;
                }
            }

            LlamaCppChatModel chatModel = new LlamaCppChatModel
            {
                BotName = botName,
                UserName = userName,
                UserContent = text,
                BotContent = content
            };
            if (model.Chats.Count > LlamaCppConfig.MemoryLimit)
            {
                SaveDbValue(key, "");
            }
            else
            {
                model.Chats.Add(chatModel);
                SaveDbValue(key, Json.ToJsonString(model));
            }
            
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
