using CQHttp.DTOs;

namespace CQHttp
{
    public static class CQAPIGroup
    {
        /// <summary>
        /// 处理加群请求／邀请
        /// </summary>
        /// <param name="flag">加群请求的 flag（需从上报的数据中获得）</param>
        /// <param name="sub_type">add 或 invite, 请求类型（需要和上报消息中的 sub_type 字段相符）</param>
        /// <param name="approve">是否同意请求／邀请</param>
        /// <param name="reason">拒绝理由（仅在拒绝时有效）</param>
        public static void Group_SetGroupAddRequest(this CQBot bot, string flag, string sub_type, bool approve = true, string reason = "")
        {
            CQRequest request = new CQRequest("set_group_add_request");
            var context = new CQAPIContext(request);
            request.@params = Json.ToJsonNode(new { flag, sub_type, approve, reason });
            bot.Send(context);
        }
    }
}
