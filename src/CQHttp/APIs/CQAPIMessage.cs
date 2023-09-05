using CQHttp.DTOs;
using System;

namespace CQHttp
{
    /// <summary>
    /// 消息
    /// </summary>
    public static class CQAPIMessage
    {
        /// <summary>
        /// 发送消息
        /// </summary>
        public static void Message_SendMsg(this CQBot bot, CQRequestMessage msg, Action<CQResponseMessage> callback = null)
        {
            CQRequest request = new CQRequest("send_msg");
            var context = new CQAPIContext(request);
            context.SetCallBack(callback);
            request.echo = context.Id;
            request.@params = Json.ToJsonNode(msg);
            bot.Send(context);
        }


        /// <summary>
        /// 快速回复消息
        /// </summary>
        public static void Message_QuickReply(this CQBot bot, CQEventMessageEx msg, CQCode cqCode, Action<CQResponseMessage> callback = null)
        {
            CQRequestMessage reqMsg = new CQRequestMessage();
            reqMsg.message_type = msg.message_type;
            if (msg.message_type == CQMessageType.Private)
            {
                reqMsg.user_id = msg.user_id;
            }
            else if (msg.message_type == CQMessageType.Group)
            {
                reqMsg.group_id = msg.group_id;
            }
            else return;

            CQRequest request = new CQRequest("send_msg");
            var context = new CQAPIContext(request);
            context.SetCallBack(callback);
            request.echo = context.Id;
            reqMsg.message = cqCode.ToJson();
            request.@params = Json.ToJsonNode(reqMsg);
            bot.Send(context);
        }

        /// <summary>
        /// 快速回复消息
        /// </summary>
        private static void Message_QuickReply(this CQBot bot, CQEventMessageEx msg, object operation)
        {
            CQRequest request = new CQRequest(".handle_quick_operation");
            var context = new CQAPIContext(request);
            request.@params = Json.ToJsonNode(new { context = msg, operation });
            bot.Send(context);
        }

        /// <summary>
        /// 快速回复私聊消息
        /// </summary>
        /// <param name="msg"></param>
        public static void Message_PrivateQuickReply(this CQBot bot, CQEventMessageEx msg, string reply)
        {
            Message_QuickReply(bot, msg, new { reply, auto_escape = false });
        }

        /// <summary>
        /// 快速回复群消息
        /// </summary>
        /// <param name="msg"></param>
        public static void Message_GroupQuickReply(this CQBot bot, CQEventMessageEx msg, CQEventQuickOperation_GroupMessage groupMessage)
        {
            Message_QuickReply(bot, msg, groupMessage);
        }

        /// <summary>
        /// 撤回消息
        /// </summary>
        /// <param name="message_id">消息 ID</param>
        public static void Message_DeleteMsg(this CQBot bot, int message_id)
        {
            CQRequest request = new CQRequest("delete_msg");
            var context = new CQAPIContext(request);
            request.@params = Json.ToJsonNode(new { message_id });
            bot.Send(context);
        }

        /// <summary>
        /// 标记消息已读
        /// </summary>
        /// <param name="message_id">消息 ID</param>
        public static void Message_MarkMsgAsRead(this CQBot bot, int message_id)
        {
            CQRequest request = new CQRequest("mark_msg_as_read");
            var context = new CQAPIContext(request);
            request.@params = Json.ToJsonNode(new { message_id });
            bot.Send(context);
        }
    }
}
