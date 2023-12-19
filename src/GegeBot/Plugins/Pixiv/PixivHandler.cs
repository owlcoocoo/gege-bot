using CQHttp;
using CQHttp.DTOs;
using System;

namespace GegeBot.Plugins.Pixiv
{
    [Plugin]
    public class PixivHandler
    {
        readonly CQBot cqBot;
        readonly PixivAPI pixivAPI = new PixivAPI();

        Dictionary<string, string> rankDict = new()
                    {
                        { "今日", "daily" },
                        { "本周", "weekly" },
                        { "本月", "monthly" },
                        { "新人", "rookie" },
                        { "原创", "original" },
                        { "AI生成", "daily_ai" },
                        { "受男性欢迎", "male" },
                        { "受女性欢迎", "female" },
                    };

        public PixivHandler(CQBot bot)
        {
            cqBot = bot;
            cqBot.ReceivedMessage += CqBot_ReceivedMessage;
        }

        private void GetUserNameAndTag(string keyword, out string user, out string tag)
        {
            user = keyword;
            tag = "";
            int pos = keyword.LastIndexOf("#");
            if (pos > -1)
            {
                user = keyword[..pos];
                tag = keyword[(pos + 1)..];
            }
        }

        private int GetIllustIndex(CQEventMessageEx msg, string id, out string key)
        {
            key = "";
            int index = 0;
            if (!string.IsNullOrEmpty(id))
            {
                key = $"{BotSession.GetSessionKey(msg)}_{id}";
                int.TryParse(PixivDb.Db.GetValue(key), out index);
            }
            return index;
        }

        private void SaveIllustIndex(string key, int index, int imageCount)
        {
            if (!string.IsNullOrEmpty(key))
            {
                index += PixivConfig.MaxImages;
                if (index >= imageCount) index = 0;
                PixivDb.Db.SetValue(key, index.ToString());
            }
        }

        private void CqBot_ReceivedMessage(CQEventMessageEx obj)
        {
            string text = CQCode.GetText(obj.message, out var atList, out _).TrimStart();
            if (atList.Count > 0 && !atList.Contains(obj.self_id.ToString())) return;

            if (text.StartsWith("搜"))
            {
                Console.WriteLine($"[pixiv]处理 {text}");

                string keyword = text[1..];

                PixivDto dto;
                if (keyword.StartsWith("帮助"))
                {
                    dto = new PixivDto();
                    string rank = "";
                    foreach (var item in rankDict) rank += item.Key + "|";
                    rank = rank.TrimEnd('|');
                    dto.Message = "pixiv 搜图帮助\r\n\r\n"
                                  + "id 格式：id + 作品或画师的一窜数字，如 id123456789\r\n\r\n"
                                  + "搜[关键字|id] —— 取 关键字 或 id 的随机一个作品\r\n\r\n"
                                  + "搜画师[name|id]  —— 取 画师名 或 画师id 的随机一个作品\r\n"
                                  + "搜画师[name|id]#tag  —— 取 画师名 或 画师id 的 标签关键字tag 的随机一个作品\r\n\r\n"
                                  + $"搜排行榜[mode]  —— 取排行榜前十的作品，mode 关键字如下：{rank}\r\n\r\n"
                                  + $"搜预览[关键字]  —— 取 关键字 作品列表的预览图\r\n"
                                  + $"搜预览画师[name|id]  —— 取 画师名 或 画师id 的作品列表的预览图\r\n"
                                  + $"搜预览画师[name|id]#tag  —— 取 画师名 或 画师id 的 标签关键字tag 的作品列表的预览图";
                }
                else if (keyword.StartsWith("画师"))
                {
                    string key = "";
                    int index = 0;

                    keyword = keyword[2..];
                    GetUserNameAndTag(keyword, out string user, out string tag);

                    if (keyword.StartsWith("id", true, null))
                    {
                        string id = keyword[2..].Trim();
                        dto = pixivAPI.Touch_SearchUserIllusts(null, id, tag, id => index = GetIllustIndex(obj, id, out key));
                    }
                    else
                    {
                        dto = pixivAPI.Touch_SearchUserIllusts(user, tag: tag, action: id => index = GetIllustIndex(obj, id, out key));
                    }

                    SaveIllustIndex(key, index, dto.ImageCount);
                }
                else if (keyword.StartsWith("排行榜"))
                {
                    keyword = keyword[3..].Trim();
                    if (rankDict.TryGetValue(keyword, out string value))
                    {
                        dto = pixivAPI.Touch_GetRankIllusts(value);
                    }
                    else
                    {
                        dto = new PixivDto() { Message = "排行榜关键字匹配失败" };
                    }
                }
                else if (keyword.StartsWith("预览"))
                {
                    keyword = keyword[2..];
                    if (keyword.StartsWith("画师"))
                    {
                        keyword = keyword[2..];
                        GetUserNameAndTag(keyword, out string user, out string tag);
                        if (keyword.StartsWith("id", true, null))
                        {
                            string id = keyword[2..].Trim();
                            dto = pixivAPI.Touch_GetUserIllustsPreview(null, id, tag);
                        }
                        else
                        {
                            dto = pixivAPI.Touch_GetUserIllustsPreview(user, tag);
                        }
                    }
                    else
                    {
                        dto = pixivAPI.Touch_GetIllustsPreview(keyword);
                    }
                }
                else
                {
                    string key = "";
                    int index = 0;

                    if (keyword.StartsWith("id", true, null))
                    {
                        string id = keyword[2..].Trim();

                        index = GetIllustIndex(obj, id, out key);

                        dto = pixivAPI.Touch_GetIllusts(id, index);
                    }
                    else
                    {
                        dto = pixivAPI.Touch_SearchIllusts(keyword, id => index = GetIllustIndex(obj, id, out key));
                    }

                    SaveIllustIndex(key, index, dto.ImageCount);
                }

                if (!string.IsNullOrEmpty(dto.Message))
                {
                    cqBot.Message_QuickReply(obj, new CQCode(dto.Message).SetReply(obj.message_id));
                    return;
                }

                Console.WriteLine($"[pixiv]发送消息");

                CQCode cqCode = new CQCode();
                if (!string.IsNullOrEmpty(dto.Alt))
                {
                    cqCode.SetReply(obj.message_id);
                    cqCode.SetText(dto.Alt);
                    cqBot.Message_QuickReply(obj, cqCode);
                    cqCode.Clear();
                }

                cqCode.SetReply(obj.message_id);
                if (!string.IsNullOrEmpty(dto.ImageMessage))
                    cqCode.SetText(dto.ImageMessage);
                foreach (var img in dto.Images)
                {
                    string base64Img = $"base64://{Convert.ToBase64String(img)}";
                    cqCode.SetImage(base64Img);
                }
                cqBot.Message_QuickReply(obj, cqCode);
            }

        }
    }
}
