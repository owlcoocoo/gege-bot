namespace CQHttp.DTOs
{
    public class CQGroupMemberInfo
    {
        /// <summary>
        /// 群号
        /// </summary>
        public long group_id { get; set; }
        /// <summary>
        /// QQ 号
        /// </summary>
        public long user_id { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string nickname { get; set; }
        /// <summary>
        /// 群名片／备注
        /// </summary>
        public string card { get; set; }
        /// <summary>
        /// 性别, male 或 female 或 unknown
        /// </summary>
        public string sex { get; set; }
        /// <summary>
        /// 年龄
        /// </summary>
        public int age { get; set; }
        /// <summary>
        /// 地区
        /// </summary>
        public string area { get; set; }
        /// <summary>
        /// 加群时间戳
        /// </summary>
        public int join_time { get; set; }
        /// <summary>
        /// 最后发言时间戳
        /// </summary>
        public long last_sent_time { get; set; }
        /// <summary>
        /// 成员等级
        /// </summary>
        public string level { get; set; }
        /// <summary>
        /// 角色, owner 或 admin 或 member
        /// </summary>
        public string role { get; set; }
        /// <summary>
        /// 是否不良记录成员
        /// </summary>
        public bool unfriendly { get; set; }
        /// <summary>
        /// 专属头衔
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// 专属头衔过期时间戳
        /// </summary>
        public long title_expire_time { get; set; }
        /// <summary>
        /// 是否允许修改群名片
        /// </summary>
        public bool card_changeable { get; set; }
        /// <summary>
        /// 禁言到期时间
        /// </summary>
        public long shut_up_timestamp { get; set; }
    }
}
