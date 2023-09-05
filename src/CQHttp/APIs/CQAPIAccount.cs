using CQHttp.DTOs;
using System;

namespace CQHttp
{
    /// <summary>
    /// Bot 账号
    /// </summary>
    public static class CQAPIAccount
    {
        /// <summary>
        /// 获取登录号信息
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="callback"></param>
        public static void Account_GetLoginInfo(this CQBot bot, Action<CQLoginInfo> callback)
        {
            CQRequest request = new CQRequest("get_login_info");
            var context = new CQAPIContext(request);
            context.SetCallBack(callback);
            request.echo = context.Id;
            bot.Send(context);
        }

        /// <summary>
        /// 设置登录号资料
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="profile"></param>
        public static void Account_SetQQProfile(this CQBot bot, CQProfile profile)
        {
            CQRequest request = new CQRequest("set_qq_profile");
            var context = new CQAPIContext(request);
            request.@params = Json.ToJsonNode(profile);
            bot.Send(context);
        }
    }
}
