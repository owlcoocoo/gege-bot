using CQHttp;
using CQHttp.DTOs;

namespace GegeBot.Plugins.Pixiv
{
    [Plugin]
    public class PixivImpl
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

        public PixivImpl(CQBot bot)
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

        private void CqBot_ReceivedMessage(CQEventMessageEx obj)
        {
            string text = CQCode.GetText(obj.message, out var atList).TrimStart();
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
                                  + "搜xxx —— 取关键字xxx的随机一个作品\r\n\r\n"
                                  + "搜画师aaa  —— 取画师aaa的随机一个作品\r\n"
                                  + "搜画师aaa#bbb  —— 取画师aaa的标签关键字bbb的随机一个作品\r\n\r\n"
                                  + $"搜排行榜xxx  —— 取排行榜前十的作品，xxx关键字如下：{rank}\r\n\r\n"
                                  + $"搜预览xxx  —— 取关键字xxx作品列表的预览图\r\n"
                                  + $"搜预览画师aaa  —— 取画师aaa的作品列表的预览图\r\n"
                                  + $"搜预览画师aaa#bbb  —— 取画师aaa的标签关键字bbb的作品列表的预览图";
                }
                else if (keyword.StartsWith("画师"))
                {
                    keyword = keyword[2..];

                    GetUserNameAndTag(keyword, out string user, out string tag);
                    dto = pixivAPI.Touch_SearchUserIllusts(user, tag);
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
                        dto = pixivAPI.Touch_GetUserIllustsPreview(user, tag);
                    }
                    else
                    {
                        dto = pixivAPI.Touch_GetIllustsPreview(keyword);
                    }
                }
                else
                {
                    dto = pixivAPI.Touch_SearchIllusts(keyword);
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
                foreach (var img in dto.Images)
                {
                    cqCode.SetImage(img);
                }
                cqBot.Message_QuickReply(obj, cqCode);
            }

        }
    }
}
