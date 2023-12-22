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
            cqBot.ReceivedGroupBan += CqBot_ReceivedGroupBan;
        }

        public void Reload()
        {
        }

        bool HandleGroupBan(CQEventMessageEx msg)
        {
            //if (msg.message_type != CQMessageType.Group)
            //    return false;

            //CQGroupMemberInfo memberInfo = cqBot.Group_GetGroupMemberInfoSync(msg.group_id, msg.self_id, true).Result;
            //if (memberInfo != null)
            //{
            //    if (memberInfo.shut_up_timestamp > 0)
            //    {
            //        Console.WriteLine($"[GroupBan]Bot 在 {msg.group_id} 已被禁言，取消处理消息。");
            //        return true;
            //    }
            //}

            return false;
        }

        private void CqBot_ReceivedGroupBan(CQEventGroupBan obj)
        {
            if (obj.sub_type == CQEventGroupBanSubType.Ban)
            {
                if (obj.user_id.ToString() == cqBot.BotID)
                {
                    cqBot.SetGroupBanned(obj.group_id.ToString(), true);
                    Console.WriteLine($"[GroupBan]Bot 在群 {obj.group_id} 被 {obj.operator_id} 禁言 {obj.duration} 秒。");
                }
                else if (obj.user_id == 0)
                {
                    Console.WriteLine($"[GroupBan]Bot 在群 {obj.group_id} 被 {obj.operator_id} 全员禁言。");
                    cqBot.SetGroupBanned(obj.group_id.ToString(), true);
                }
            }
            else if (obj.sub_type == CQEventGroupBanSubType.LiftBan)
            {
                if (obj.user_id == 0 || obj.user_id.ToString() == cqBot.BotID)
                {
                    Console.WriteLine($"[GroupBan]Bot 在群 {obj.group_id} 被 {obj.operator_id} 解除禁言。");
                    cqBot.SetGroupBanned(obj.group_id.ToString(), false);
                }
            }
        }
    }
}
