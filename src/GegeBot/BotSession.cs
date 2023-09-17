using CQHttp.DTOs;

namespace GegeBot
{
    internal class BotSession
    {
        public static string GetSessionKey(CQEventMessageEx msg)
        {
            string key = "";
            if (msg.message_type == CQMessageType.Private)
            {
                key = $"{CQMessageType.Private}_{msg.user_id}";
            }
            else if (msg.message_type == CQMessageType.Group)
            {
                key = $"{CQMessageType.Group}_{msg.group_id}";
            }
            return key;
        }

    }
}
