using CQHttp;
using CQHttp.DTOs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GegeBot.Plugins.GroupBan
{
    public class GroupBanHandler : IPlugin
    {
        readonly CQBot cqBot;

        public GroupBanHandler(CQBot bot)
        {
            cqBot = bot;
            cqBot.HandleGroupBan = HandleGroupBan;
        }

        public void Reload()
        {
        }

        bool HandleGroupBan(CQEventMessageEx msg)
        {
            if (msg.message_type != CQMessageType.Group)
                return false;

            CQGroupMemberInfo memberInfo = cqBot.Group_GetGroupMemberInfoSync(msg.group_id, msg.self_id).Result;
            if (memberInfo != null)
            {
                if (memberInfo.shut_up_timestamp > 0)
                {
                    Console.WriteLine($"[GroupBan]Bot 在 {msg.group_id} 已被禁言，取消处理消息。");
                    return true;
                }
            }

            return false;
        }
    }
}
