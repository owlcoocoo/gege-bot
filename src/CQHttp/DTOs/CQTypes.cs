namespace CQHttp.DTOs
{
    public struct CQMessageType
    {
        public const string Private = "private";
        public const string Group = "group";
    }


    public struct CQSubType
    {
        public const string Public = "public";
        public const string Group = "group";
    }

    public struct CQPrivateMessageSubType
    {
        /// <summary>
        /// 好友
        /// </summary>
        public const string Friend = "friend";
        /// <summary>
        /// 群临时会话
        /// </summary>
        public const string Group = "group";
        /// <summary>
        /// 在群中自身发送
        /// </summary>
        public const string GroupSelf = "group_self";

        public const string Other = "other";
    }

    public struct CQRequestType
    {
        /// <summary>
        /// 好友
        /// </summary>
        public const string Friend = "friend";
        /// <summary>
        /// 群
        /// </summary>
        public const string Group = "group";
    }

    public struct CQRequestGroupSubType
    {
        /// <summary>
        /// 加群请求
        /// </summary>
        public const string Add = "add";
        /// <summary>
        /// 邀请入群
        /// </summary>
        public const string Invite = "invite";
    }

    public struct CQGruopMessageSubType
    {
        /// <summary>
        /// 正常消息
        /// </summary>
        public const string Normal = "normal";
        /// <summary>
        /// 匿名消息
        /// </summary>
        public const string Anonymous = "anonymous";
        /// <summary>
        /// 系统提示 ( 如「管理员已禁止群内匿名聊天」 )
        /// </summary>
        public const string Notice = "notice";
    }

    public struct CQMetaEventType
    {
        public const string Lifecycle = "lifecycle";
        public const string Heartbeat = "heartbeat";
    }

    public struct CQPostType
    {
        /// <summary>
        /// 消息
        /// </summary>
        public const string Message = "message";
        /// <summary>
        /// 消息发送
        /// </summary>
        public const string MessageSent = "message_sent";
        /// <summary>
        /// 请求
        /// </summary>
        public const string Request = "request";
        /// <summary>
        /// 通知
        /// </summary>
        public const string Notice = "notice";
        /// <summary>
        /// 元事件
        /// </summary>
        public const string MetaEvent = "meta_event";
    }

    public struct CQEventNoticeType
    {
        /// <summary>
        /// 群禁言
        /// </summary>
        public const string GroupBan = "group_ban";

        public const string Notify = "notify";
    }

    public struct CQEventNotifySubType
    {
        /// <summary>
        /// 戳一戳
        /// </summary>
        public const string Poke = "poke";
    }

    public struct CQEventGroupBanSubType
    {
        /// <summary>
        /// 禁言
        /// </summary>
        public const string Ban = "ban";
        /// <summary>
        /// 解除禁言
        /// </summary>
        public const string LiftBan = "lift_ban";
    }
}
