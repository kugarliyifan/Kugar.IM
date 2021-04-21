using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Kugar.Core.Web;
using Kugar.IM.DB.DTO;
using Kugar.IM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kugar.IM.Server.Areas.IM.Controllers
{
    [Authorize]
    public class MessageController : IMBaseController
    {
        [HttpPost,FromBodyJson]
        public async Task<ResultReturn> SetMessageReadState(
            long messageId,
            [FromServices]MessageService message,
            [FromServices]SessionService session
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

        [HttpGet]
        public async Task<ResultReturn<IEnumerable<DTO_SessionMessage>>> GetMessages(
            [FromServices]MessageService message,
            [FromServices]SessionService session,
            [FromQuery] long sessionId,
            [FromQuery]long lastMessageId=-1)
        {
            if (!await session.IsUserExistsInSession(sessionId,CurrentUserId))
            {
                return new FailResultReturn<IEnumerable<DTO_SessionMessage>>("无效sessionId");
            }

            var lst =await message.GetSessionMessage(sessionId, lastMessageId);

            return new SuccessResultReturn<IEnumerable<DTO_SessionMessage>>(lst.Reverse());
        }
    }
}