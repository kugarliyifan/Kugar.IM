using System.Collections.Generic;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Kugar.Core.Web;
using Kugar.Core.Web.JsonTemplate.Helpers;
using Kugar.IM.DB.DTO;
using Kugar.IM.Server.Areas.IM.Views.Session;
using Kugar.IM.Services;
using Kugar.IM.Web.Hub;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;

namespace Kugar.IM.Web.Controllers
{
    public class SessionController : IMBaseController
    {
        /// <summary>
        /// 获取当前用户的所有会话
        /// </summary>
        /// <param name="session"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet,ProducesResponseType(typeof(View_GetSessions),200)]
        public async Task<IActionResult> GetSessions(
            [FromServices] ISessionService session, 
            [FromQuery] int pageIndex = 1, 
            [FromQuery] int pageSize = 20)
        {
            var lst = await session.GetUserSessions(CurrentUserId, pageIndex, pageSize);

            return this.Json<View_GetSessions>(lst);

            //return new SuccessResultReturn<IPagedList<DTO_UserSession>>(lst);
        }

        /// <summary>
        /// 创建一个一对一的会话
        /// </summary>
        /// <param name="session"></param>
        /// <param name="toUserId"></param>
        /// <returns></returns>
        [FromBodyJson, HttpPost]
        public async Task<ResultReturn<long>> CreateOneToOneSession(
            [FromServices] ISessionService session,
            string toUserId)
        {
            var sessionId = await session.CreateOneToOneSession(CurrentUserId, toUserId);

            return new SuccessResultReturn<long>(sessionId);
        }

        /// <summary>
        /// 创建一个群组会话,并拉指定人员入群
        /// </summary>
        /// <param name="callContext"></param>
        /// <param name="hubContext"></param>
        /// <param name="session"></param>
        /// <param name="toUserIds"></param>
        /// <returns></returns>
        [FromBodyJson, HttpPost]
        public async Task<ResultReturn<DTO_UserSession>> CreateGroupSession(
            [FromServices]HubCallerContext callContext,
            [FromServices]IHubContext<ChatHub> hubContext,
            [FromServices]IMemoryCache cache,
            [FromServices] ISessionService session, 
            string[] toUserIds)
        {
            var sessionId = await session.CreateGroupSession(CurrentUserId, toUserIds);

            var sessionInfo = await session.GetSessionInfoById(sessionId);

            await hubContext.Groups.AddToGroupAsync(callContext.ConnectionId,sessionId.ToStringEx());

            foreach (var userId in toUserIds)
            {
                if (cache.TryGetValue(userId,out var tmp))
                {
                    await hubContext.Groups.AddToGroupAsync(tmp.ToStringEx(),sessionId.ToStringEx());
                }
            }

            return new SuccessResultReturn<DTO_UserSession>(sessionInfo);
        }

        /// <summary>
        /// 加入一个会话
        /// </summary>
        /// <param name="callContext"></param>
        /// <param name="hubContext"></param>
        /// <param name="session"></param>
        /// <param name="sessionId">会话ID</param>
        /// <returns></returns>
        [FromBodyJson,HttpPost]
        public async Task<ResultReturn> JoinToSession([FromServices] HubCallerContext callContext,
            [FromServices] IHubContext<ChatHub> hubContext,
            [FromServices] ISessionService session,
            long sessionId)
        {
            var ret = await session.JoinToSession(CurrentUserId, sessionId);

            if (ret.IsSuccess)
            {
                await hubContext.Groups.AddToGroupAsync(callContext.ConnectionId, sessionId.ToStringEx());
            }
            
            return ret;
        }

        [HttpGet]
        public async Task<ResultReturn<IReadOnlyList<DTO_UserSessionUnReadCount>>> GetUnReadCount([FromServices] ISessionService session)
        {
            var lst = await session.GetUserSessionUnReadCount(CurrentUserId);

            return new SuccessResultReturn<IReadOnlyList<DTO_UserSessionUnReadCount>>(lst);
        }
    }
}