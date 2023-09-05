namespace CQHttp.DTOs
{
    public class CQStatusStatistics
    {
        /// <summary>
        /// 收包数
        /// </summary>
        public ulong packet_received { get; set; }
        /// <summary>
        /// 发包数
        /// </summary>
        public ulong packet_sent { get; set; }
        /// <summary>
        /// 丢包数
        /// </summary>
        public ulong packet_lost { get; set; }
        /// <summary>
        /// 消息接收数
        /// </summary>
        public ulong message_recei { get; set; }
        /// <summary>
        /// 消息发送数
        /// </summary>
        public ulong message_sent { get; set; }
        /// <summary>
        /// 连接断开次数
        /// </summary>
        public uint disconnect_times { get; set; }
        /// <summary>
        /// 连接丢失次数
        /// </summary>
        public uint lost_times { get; set; }
        /// <summary>
        /// 最后一次消息时间
        /// </summary>
        public ulong last_message_time { get; set; }
    }

    public class CQStatus
    {
        /// <summary>
        /// 程序是否初始化完毕
        /// </summary>
        public bool app_initialized { get; set; }
        /// <summary>
        /// 程序是否可用

        /// </summary>
        public bool app_enabled { get; set; }
        /// <summary>
        /// 插件正常(可能为 null)
        /// </summary>
        public bool? plugins_good { get; set; }
        /// <summary>
        /// 程序正常
        /// </summary>
        public bool app_good { get; set; }
        /// <summary>
        /// 是否在线
        /// </summary>
        public bool online { get; set; }
        /// <summary>
        /// 统计信息
        /// </summary>
        public CQStatusStatistics stat { get; set; }
    }

    public class CQEventHeartbeat : CQEventBase
    {
        /// <summary>
        /// 距离上一次心跳包的时间(单位是毫秒)
        /// </summary>
        public long interval { get; set; }
        /// <summary>
        /// 应用程序状态
        /// </summary>
        public CQStatus status { get; set; }
    }
}
