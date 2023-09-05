namespace CQHttp.DTOs
{

    public class CQEventMessage : CQEventBase
    {
        /// <summary>
        /// 消息类型
        /// <para>private, group</para>
        /// </summary>
        public string message_type { get; set; }
        /// <summary>
        /// 表示消息的子类型
        /// <para>group, public</para>
        /// </summary>
        public string sub_type { get; set; }
        /// <summary>
        /// 消息 ID
        /// </summary>
        public int message_id { get; set; }
        /// <summary>
        /// 起始消息序号
        /// </summary>
        public long message_seq { get; set; }
        /// <summary>
        /// 发送者 QQ 号
        /// </summary>
        public long user_id { get; set; }
        /// <summary>
        /// 一个消息链
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// CQ 码格式的消息
        /// </summary>
        public string raw_message { get; set; }
        /// <summary>
        /// 字体
        /// </summary>
        public int font { get; set; }
        /// <summary>
        /// 发送者信息
        /// </summary>
        public CQEvnetMessageSender sender { get; set; }

    }

    public class CQEvnetMessageSender
    {
        /// <summary>
        /// 发送者 QQ 号
        /// </summary>
        public long user_id { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string nickname { get; set; }
        /// <summary>
        /// 性别, male 或 female 或 unknown
        /// </summary>
        public string sex { get; set; }
        /// <summary>
        /// 年龄
        /// </summary>
        public int age { get; set; }
        /// <summary>
        /// 临时群消息来源群号
        /// </summary>
        public int? group_id { get; set; }
        /// <summary>
        /// 群名片／备注
        /// </summary>
        public string card { get; set; }
        /// <summary>
        /// 地区
        /// </summary>
        public string area { get; set; }
        /// <summary>
        /// 成员等级
        /// </summary>
        public string level { get; set; }
        /// <summary>
        /// 角色, owner 或 admin 或 member
        /// </summary>
        public string role { get; set; }
        /// <summary>
        /// 专属头衔
        /// </summary>
        public string title { get; set; }
    }

    public class CQEventMessageAnonymous
    {
        /// <summary>
        /// 匿名用户 ID
        /// </summary>
        public long id { get; set; }
        /// <summary>
        /// 匿名用户名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 匿名用户 flag, 在调用禁言 API 时需要传入
        /// </summary>
        public string flag { get; set; }
    }

    public class CQEventMessageEx : CQEventMessage
    {
        /// <summary>
        /// 接收者 QQ 号
        /// </summary>
        public long target_id { get; set; }
        /// <summary>
        /// 临时会话来源
        /// </summary>
        public int temp_source { get; set; }
        /// <summary>
        /// 群号
        /// </summary>
        public long group_id { get; set; }
        /// <summary>
        /// 匿名信息, 如果不是匿名消息则为 null
        /// </summary>
        public CQEventMessageAnonymous anonymous { get; set; }
    }
}
