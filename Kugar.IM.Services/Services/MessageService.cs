using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FreeSql;
using Kugar.Core.BaseStruct;
using Kugar.IM.DB.DTO;
using Kugar.IM.DB.Entities;
using Kugar.IM.DB.Enums;

namespace Kugar.IM.Services
{
    public interface IMessageService
    {
        /// <summary>
        /// 添加一条信息
        /// </summary>
        /// <param name="fromUserId"></param>
        /// <param name="toSessionId"></param>
        /// <param name="content"></param>
        /// <param name="type"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        Task<long> AddMessage(
            string fromUserId, 
            long toSessionId, 
            string content, 
            MessageTypeEnum type,
            MessageContentTypeEnum contentType);

        /// <summary>
        /// 撤回信息
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        Task RevocationMessage(long messageId);

        /// <summary>
        /// 设置会话的阅读时间
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sessionId"></param>
        /// <param name="lastMessageId"></param>
        /// <returns></returns>
        Task SetMessageReadState(string userId,long sessionId, long lastMessageId);

        /// <summary>
        /// 拉取历史记录
        /// </summary>
        /// <param name="sessionId">会话Id</param>
        /// <param name="lastMessageId">上一次拉取的消息Id,如果为-1,则为从最新开始拉取,如果>0,则为最后一条消息id,并从这条消息开始往前拉取</param>
        /// <returns></returns>
        Task<IReadOnlyList<DTO_SessionMessage>> GetSessionMessage(long sessionId, long lastMessageId=-1, int takeCount=20);

        Task<long> GetMessageSessionById(long messageId);
    }

    /// <summary>
    /// 消息服务
    /// </summary>
    public class MessageService:BaseService, IMessageService
    {
        private SessionService _sessionService = null;

        public MessageService(IFreeSql freeSql,SessionService sessionService) : base(freeSql)
        {
            _sessionService = sessionService;
        }
        
        /// <summary>
        /// 添加一条信息
        /// </summary>
        /// <param name="fromUserId"></param>
        /// <param name="toSessionId"></param>
        /// <param name="content"></param>
        /// <param name="type"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public async Task<long> AddMessage(
            string fromUserId, 
            long toSessionId, 
            string content, 
            MessageTypeEnum type,
            MessageContentTypeEnum contentType)
        {
            if (!await FreeSql.Select<im_chat_session>().Where(x=>x.SessionId==toSessionId).AnyAsync())
            {
                return -1;
            }

            if (!await FreeSql.Select<im_chat_userInSession>().Where(x=>x.SessionId==toSessionId && x.UserId==fromUserId).AnyAsync())
            {
                return -1;
            }

            
            var item = new im_chat_message()
            {
                SessionId = toSessionId,
                SenderId = fromUserId,
                Content = content,
                ContentType = contentType,
                SendDt = DateTime.Now,
                Type = type
            };

            var messageId= await FreeSql.Insert(item).ExecuteIdentityAsync();

            await FreeSql.Update<im_chat_session>()
                .Where(x => x.SessionId == toSessionId)
                .Set(x => x.LastActivityDt, DateTime.Now)
                .Set(x=>x.LastMessageId,messageId)
                .ExecuteUpdatedAsync();

            return messageId;
        }

        /// <summary>
        /// 撤回信息
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public async Task RevocationMessage(long messageId)
        {
            await FreeSql.Update<im_chat_message>().Where(x => x.MessageId == messageId).Set(x => x.State, 99)
                .ExecuteAffrowsAsync();
        }

        /// <summary>
        /// 设置会话的阅读时间
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sessionId"></param>
        /// <param name="lastMessageId"></param>
        /// <returns></returns>
        public async Task SetMessageReadState(string userId,long sessionId, long lastMessageId)
        {
            //var sessionId =await GetMessageSessionById(lastMessageId);

            //if (sessionId<=0)
            //{
            //    return;
            //}
    
            await FreeSql.InsertOrUpdate<im_chat_userMessageStatus>().SetSource(new im_chat_userMessageStatus()
            {
                LastReadMessageId = lastMessageId,
                SessionId = sessionId,
                UserId = userId
            }).ExecuteAffrowsAsync();
        }

        /// <summary>
        /// 拉取历史记录
        /// </summary>
        /// <param name="sessionId">会话Id</param>
        /// <param name="lastMessageId">上一次拉取的消息Id,如果为-1,则为从最新开始拉取,如果>0,则为最后一条消息id,并从这条消息开始往前拉取</param>
        /// <returns></returns>
        public async Task<IReadOnlyList<DTO_SessionMessage>> GetSessionMessage(long sessionId, long lastMessageId=-1, int takeCount=20)
        {
            return await FreeSql.Select<im_chat_message>().Where(x => x.SessionId == sessionId && x.State==0)
                .WhereIf(lastMessageId > 0, x => x.MessageId < lastMessageId)
                .OrderByDescending(x => x.MessageId)
                .Take(takeCount)
                .ToListAsync(x => new DTO_SessionMessage()
                {
                    Content =x.State==99?"已撤回的消息":x.Content,
                    ContentType = x.ContentType,
                    SendDt = x.SendDt,
                    SendUserId = x.SenderId,
                    MessageType = x.Type,
                    State = x.State
                });
        }

        public async Task<long> GetMessageSessionById(long messageId)
        {
            return await FreeSql.Select<im_chat_message>().Where(x => x.MessageId == messageId)
                .ToOneAsync(x => x.SessionId);
            ;
        }
    }
}
