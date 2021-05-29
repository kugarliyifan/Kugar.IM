using System;
using System.Linq;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Kugar.IM.DB.Enums;
using Kugar.IM.Services;
using Kugar.IM.Web.Cache;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;

namespace Kugar.IM.Web.Hub
{
    [Authorize(AuthenticationSchemes = "im"),]
    public class ChatHub : Microsoft.AspNetCore.SignalR.Hub
    {
        private ISessionService _session;
        private IIMConnectionManager _connManager;
        private IMessageService _message;

        public ChatHub(ISessionService session, IMessageService message, IIMConnectionManager connManager)
        {
            _session = session;
            _connManager = connManager;
            _message = message;
        }

        [Authorize(AuthenticationSchemes = "im"),]
        public async Task<ResultReturn> SendMessage(
                //[FromServices]MessageService message, 
                //[FromServices]SessionService session,
                //[FromServices]IMemoryCache cache,
                long sessionId,
                string content,
                int contentType
                )
        {
            var messageId=await _message.AddMessage(Context.UserIdentifier, sessionId, content, MessageTypeEnum.User,
                (MessageContentTypeEnum)contentType);

            var sessionInfo = await _session.GetSessionById(sessionId);

            if (sessionInfo.Type == SessionTypeEnum.OneToOne)
            {
                var s = await _session.GetSessionUserIds(sessionId);

                var friendUserId = s.FirstOrDefault(x => x != Context.UserIdentifier);

                if (await _connManager.IsUserOnline(friendUserId))
                {
                    await Clients.User(friendUserId).SendAsync("ReceivedMessage", sessionId, content, contentType,messageId);
                }

            }
            else if (sessionInfo.Type == SessionTypeEnum.Group)
            {
                await Clients.Group(sessionInfo.SessionId.ToStringEx()).SendAsync("SendMessage", content);
            }

            return new SuccessResultReturn(messageId);
        }

        [Authorize(AuthenticationSchemes = "im"),]
        public override async Task OnConnectedAsync()
        {
            var groupIds = await _session.GetUserGroupIDs(Context.UserIdentifier);

            foreach (var groupId in groupIds)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString());
            }

            await _connManager.AddUserConnectionId(Context.UserIdentifier, Context.ConnectionId);

            await base.OnConnectedAsync();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = "im"),]
        public async Task OnKeepAlive()
        {
            await _connManager.ActivityUser(Context.UserIdentifier);

        }

        /// <summary>
        /// 离线后触发
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = "im"),]
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var groupIds = await _session.GetUserGroupIDs(Context.UserIdentifier);

            foreach (var groupId in groupIds)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId.ToString());
            }

            await _connManager.RemoveUserConnectionId(Context.UserIdentifier, Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }

    }
}
