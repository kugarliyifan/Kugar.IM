using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.Web;
using Kugar.IM.DB.DTO;
using Kugar.IM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kugar.IM.Web.Controllers
{
    /// <summary>
    /// 消息管理
    /// </summary>

    public class MessageController : IMBaseController
    {
        /// <summary>
        /// 设置消息已读状态
        /// </summary>
        /// <param name="messageId">消息ID</param>
        /// <param name="message"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        [HttpPost,FromBodyJson]
        public async Task<ResultReturn> SetMessageReadState(
            long messageId,
            [FromServices]IMessageService message,
            [FromServices]ISessionService session
            )
        {
            var sessionId = await message.GetMessageSessionById(messageId);

            if (!await session.IsUserExistsInSession(sessionId,CurrentUserId))
            {
                return new FailResultReturn("消息不存在");
            }

            await message.SetMessageReadState(CurrentUserId,sessionId,messageId);

            return SuccessResultReturn.Default;
        }

        /// <summary>
        /// 获取历史消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="session"></param>
        /// <param name="sessionId">会话id,如果需要拉取当前用户的所有消息,则为-1</param>
        /// <param name="lastMessageId">最后一条消息id</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResultReturn<IEnumerable<DTO_SessionMessage>>> GetMessages(
            [FromServices]IMessageService message,
            [FromServices]ISessionService session,
            [FromQuery] long sessionId,
            [FromQuery]long lastMessageId=-1)
        {
            if (!await session.IsUserExistsInSession(sessionId,CurrentUserId))
            {
                return new FailResultReturn<IEnumerable<DTO_SessionMessage>>("无效sessionId");
            }

            var lst =await message.GetSessionMessage(sessionId,CurrentUserId, lastMessageId);

            if (lst.Count>0)
            {
                foreach (var item in lst)
                {
                    item.IsMe = CurrentUserId == item.SendUserId;
                }
            }

            return new SuccessResultReturn<IEnumerable<DTO_SessionMessage>>(lst.Reverse());
        }
    }
}