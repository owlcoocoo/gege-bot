using CQHttp.DTOs;
using System;
using System.Threading.Tasks;

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

        /// <summary>
        /// 获取群成员信息
        /// </summary>
        /// <param name="group_id">群号</param>
        /// <param name="user_id">QQ 号</param>
        /// <param name="no_cache">是否不使用缓存（使用缓存可能更新不及时, 但响应更快）</param>
        public static void Group_GetGroupMemberInfo(this CQBot bot, long group_id, long user_id, bool no_cache = false, Action<CQGroupMemberInfo> callback = null)
        {
            CQRequest request = new CQRequest("get_group_member_info");
            var context = new CQAPIContext(request);
            context.SetCallBack(callback);
            request.echo = context.Id;
            request.@params = Json.ToJsonNode(new { group_id, user_id, no_cache });
            bot.Send(context);
        }

        /// <summary>
        /// 获取群成员信息
        /// </summary>
        /// <param name="group_id">群号</param>
        /// <param name="user_id">QQ 号</param>
        /// <param name="no_cache">是否不使用缓存（使用缓存可能更新不及时, 但响应更快）</param>
        public static CQGroupMemberInfo Group_GetGroupMemberInfoSync(this CQBot bot, long group_id, long user_id, bool no_cache = false)
        {
            CQGroupMemberInfo memberInfo = null;
            bool ret = false;
            bot.Group_GetGroupMemberInfo(group_id, user_id, no_cache, callback: result =>
            {
                memberInfo = result;
                ret = true;
            });
            while (!ret) Task.Yield().GetAwaiter().GetResult();
            return memberInfo;
        }

        /// <summary>
        /// 获取群信息
        /// </summary>
        /// <param name="group_id">群号</param>
        /// <param name="no_cache">是否不使用缓存（使用缓存可能更新不及时, 但响应更快）</param>
        public static void Group_GetGroupInfo(this CQBot bot, long group_id, Action<CQGroupInfo> callback, bool no_cache = false)
        {
            CQRequest request = new CQRequest("get_group_info");
            var context = new CQAPIContext(request);
            context.SetCallBack(callback);
            request.echo = context.Id;
            request.@params = Json.ToJsonNode(new { group_id, no_cache });
            bot.Send(context);
        }

        /// <summary>
        /// 获取群列表
        /// </summary>
        /// <param name="no_cache">是否不使用缓存（使用缓存可能更新不及时, 但响应更快）</param>
        public static void Group_GetGroupList(this CQBot bot, Action<CQGroupInfo[]> callback, bool no_cache = false)
        {
            CQRequest request = new CQRequest("get_group_list");
            var context = new CQAPIContext(request);
            context.SetCallBack(callback);
            request.echo = context.Id;
            request.@params = Json.ToJsonNode(new { no_cache });
            bot.Send(context);
        }

        /// <summary>
        /// 获取群列表
        /// </summary>
        /// <param name="group_id">群号</param>
        /// <param name="user_id">QQ 号</param>
        /// <param name="no_cache">是否不使用缓存（使用缓存可能更新不及时, 但响应更快）</param>
        public static CQGroupInfo[] Group_GetGroupListSync(this CQBot bot, bool no_cache = false)
        {
            CQGroupInfo[] groupInfo = null;
            bool ret = false;
            bot.Group_GetGroupList(result =>
            {
                groupInfo = result;
                ret = true;
            }, no_cache);
            while (!ret) Task.Yield().GetAwaiter().GetResult();
            return groupInfo;
        }
    }
}
