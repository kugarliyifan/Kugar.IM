using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kugar.Core.ExtMethod;
using Kugar.IM.DB.Enums;
using Kugar.IM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;

namespace Kugar.IM.Server.Areas.IM.Controllers
{
    [Authorize(AuthenticationSchemes = "us"),]
    public class ChatHub:Hub
    {
        private SessionService _session;
        private IMemoryCache _cache;
        private MessageService _message;

        public ChatHub( SessionService session,MessageService message, IMemoryCache cache)
        {
            _session = session;
            _cache = cache;
            _message = message;
        }

        public async Task SendMessage(
            //[FromServices]MessageService message, 
            //[FromServices]SessionService session,
            //[FromServices]IMemoryCache cache,
            long sessionId,
            string content,
            int contentType
            )
        {
            await _message.AddMessage(Context.UserIdentifier, sessionId, content, MessageTypeEnum.User,
                (MessageContentTypeEnum) contentType);

            var sessionInfo = await _session.GetSessionById(sessionId);

            if (sessionInfo.Type== SessionTypeEnum.OneToOne)
            {
                var s =await _session.GetSessionUserIds(sessionId);

                var friendUserId = s.FirstOrDefault(x => x != Context.UserIdentifier);

                if (_cache.TryGetValue(friendUserId,out _))
                {
                    await Clients.User(friendUserId).SendAsync("ReceivedMessage", content);
                }
            }
            else if(sessionInfo.Type== SessionTypeEnum.Group)
            {
                await Clients.Group(sessionInfo.SessionId.ToStringEx()).SendAsync("SendMessage", content);
            }
            
        }

        public  override async Task OnConnectedAsync()
        {
            var groupIds =await _session.GetUserGroupIDs(Context.UserIdentifier);

            foreach (var groupId in groupIds)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString());
            }

            _cache.Set(Context.UserIdentifier, Context.ConnectionId,DateTimeOffset.Now.AddMinutes(5));

            await base.OnConnectedAsync();
        } 

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task OnKeepAlive()
        {
            _cache.Set(Context.UserIdentifier,  Context.ConnectionId,DateTimeOffset.Now.AddMinutes(5));
        }
        
        /// <summary>
        /// 离线后触发
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var groupIds =await _session.GetUserGroupIDs(Context.UserIdentifier);

            foreach (var groupId in groupIds)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId.ToString());
            }

            _cache.Remove(Context.UserIdentifier);

            await base.OnDisconnectedAsync(exception);
        }
        
    }
}
