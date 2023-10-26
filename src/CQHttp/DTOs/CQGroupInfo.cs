namespace CQHttp.DTOs
{
    public class CQGroupInfo
    {
        /// <summary>
        /// 	群号
        /// </summary>
        public long group_id { get; set; }
        /// <summary>
        /// 群名称
        /// </summary>
        public string group_name { get; set; }
        /// <summary>
        /// 群备注
        /// </summary>
        public string group_memo { get; set; }
        /// <summary>
        /// 群创建时间
        /// </summary>
        public uint group_create_time { get; set; }
        /// <summary>
        /// 群等级
        /// </summary>
        public uint group_level { get; set; }
        /// <summary>
        /// 成员数
        /// </summary>
        public int member_count { get; set; }
        /// <summary>
        /// 最大成员数（群容量）
        /// </summary>
        public int max_member_count { get; set; }
    }
}
