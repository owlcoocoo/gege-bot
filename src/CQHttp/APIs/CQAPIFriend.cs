using CQHttp.DTOs;

namespace CQHttp
{
    public static class CQAPIFriend
    {
        /// <summary>
        /// 处理加好友请求
        /// </summary>
        /// <param name="flag">加好友请求的 flag（需从上报的数据中获得）</param>
        /// <param name="approve">是否同意请求</param>
        /// <param name="remark">添加后的好友备注（仅在同意时有效）</param>
        public static void Friend_SetFriendAddRequest(this CQBot bot, string flag, bool approve = true, string remark = "")
        {
            CQRequest request = new CQRequest("set_friend_add_request");
            var context = new CQAPIContext(request);
            request.@params = Json.ToJsonNode(new { flag, approve, remark });
            bot.Send(context);
        }
    }
}
