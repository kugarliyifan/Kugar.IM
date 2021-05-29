using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Kugar.IM.DB.DTO;
using Kugar.IM.DB.Entities;
using Kugar.IM.DB.Enums;

namespace Kugar.IM.Services
{
    public interface ISessionService
    {
        /// <summary>
        /// 创建一个一对一的私聊会话
        /// </summary>
        /// <param name="fromUserId"></param>
        /// <param name="toUserId"></param>
        /// <returns></returns>
        Task<long> CreateOneToOneSession(string fromUserId, string toUserId);

        /// <summary>
        /// 创建一个群聊会话
        /// </summary>
        /// <param name="fromUserId"></param>
        /// <param name="toUserIds"></param>
        /// <returns></returns>
        Task<long> CreateGroupSession(string fromUserId,string[] toUserIds);

        /// <summary>
        /// 获取用户相关连的会话列表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<IPagedList<DTO_UserSession>> GetUserSessions(string userId, int pageIndex = 1,
            int pageSize = 20);

        /// <summary>
        /// 获取用户的所有未读数量
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns></returns>
        Task<IReadOnlyList<DTO_UserSessionUnReadCount>> GetUserSessionUnReadCount(string userId);

        /// <summary>
        /// 指定用户Id加入session会话中
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        Task<ResultReturn> JoinToSession(string userId, long sessionId);

        /// <summary>
        /// 获取两个用户的私聊会话id
        /// </summary>
        /// <param name="fromUserId"></param>
        /// <param name="toUserId"></param>
        /// <returns></returns>
        Task<long> GetToUseIdSessionIdByFromUseId(string fromUserId, string toUserId);

        /// <summary>
        /// 获取指定会话的用户Id列表
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        Task<IReadOnlyList<string>> GetSessionUserIds(long sessionId);

        /// <summary>
        /// 获取指定Id会话信息
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        Task<im_chat_session> GetSessionById(long sessionId);

        /// <summary>
        /// 获取指定用户的群组Id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<IReadOnlyList<long>> GetUserGroupIDs(string userId);

        /// <summary>
        /// 获取包含用户Id列表的群组信息
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        Task<DTO_UserSession> GetSessionInfoById(long sessionId);

        /// <summary>
        /// 判断指定用户是否在指定会话中
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> IsUserExistsInSession(long sessionId, string userId);
    }

    public class SessionService:BaseService, ISessionService
    {
        public SessionService(IFreeSql freeSql) : base(freeSql)
        {
        }

        /// <summary>
        /// 创建一个一对一的私聊会话
        /// </summary>
        /// <param name="fromUserId"></param>
        /// <param name="toUserId"></param>
        /// <returns></returns>
        public async Task<long> CreateOneToOneSession(string fromUserId, string toUserId)
        {
            var sessionId = await GetToUseIdSessionIdByFromUseId(fromUserId,toUserId);

            if (sessionId>0)
            {
                return sessionId;
            }

            sessionId=await FreeSql.Insert(new im_chat_session()
            {
                CreateDt = DateTime.Now,
                CreateUserId = fromUserId,
                ExtData = $"{fromUserId},{toUserId},",
                IsPublic = false,
                Name = "",
                Type = SessionTypeEnum.OneToOne,
                

            }).ExecuteIdentityAsync();

            await FreeSql.Insert(new[]
            {
                new im_chat_userInSession()
                {
                    JoinDt = DateTime.Now,
                    Order = 0,
                    QuitDt = null,
                    SessionId = sessionId,
                    UserId = fromUserId
                },
                new im_chat_userInSession()
                {
                    JoinDt = DateTime.Now,
                    Order = 0,
                    QuitDt = null,
                    SessionId = sessionId,
                    UserId = toUserId
                }
            }).ExecuteInsertedAsync();

            return sessionId;
        }

        /// <summary>
        /// 创建一个群聊会话
        /// </summary>
        /// <param name="fromUserId"></param>
        /// <param name="toUserIds"></param>
        /// <returns></returns>
        public async Task<long> CreateGroupSession(string fromUserId,string[] toUserIds)
        {
            long sessionId = 0L;

            FreeSql.Transaction(() =>
            {
                sessionId=FreeSql.Insert(new im_chat_session()
                {
                    CreateDt = DateTime.Now,
                    CreateUserId = fromUserId,
                    IsPublic = false,
                    Name = $"群聊{DateTime.Now:yyyyMMdd}",
                    Type = SessionTypeEnum.Group
                }).ExecuteIdentity();


                FreeSql.Insert(toUserIds.Select(x => new im_chat_userInSession()
                {
                    JoinDt = DateTime.Now,
                    SessionId = sessionId,
                    UserId = x
                })).ExecuteInserted();
            });

            return sessionId;
        }

        /// <summary>
        /// 获取用户相关连的会话列表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<IPagedList<DTO_UserSession>> GetUserSessions(string userId, int pageIndex = 1,
            int pageSize = 20) 
        {
            var query = FreeSql.Select<im_chat_session, im_chat_userInSession,im_chat_message>()
                .InnerJoin((x1, x2,x3) => x1.SessionId == x2.SessionId)
                .LeftJoin((x1,x2,x3)=>x1.LastMessageId==x3.MessageId)
                .Where((x1, x2,x3) => x2.UserId == userId);

            var count = await query.CountAsync();

            if (count<=0)
            {
                return VM_PagedList<DTO_UserSession>.Empty(pageIndex, pageSize);
            }

            var t = await query
                .OrderBy((x1, x2,x3) => x2.Order)
                .OrderByDescending((x1, x2,x3) => x1.LastActivityDt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync<DTO_UserSession>((x1, x2, x3) => new DTO_UserSession()
                {
                    SessionId = x1.SessionId,
                    Name = x1.Name,
                    ContentType = x3==null?null:(int?)x3.ContentType,
                    LastMessageContent = x3==null?"":x3.Content,
                    CreateDt = x1.CreateDt,
                    CreateUserId = x1.CreateUserId,
                    ExtData = x1.ExtData,
                    HeaderImageUrl = x1.HeaderImageUrl,
                    IsPublic = x1.IsPublic,
                    LastMessageId=x1.LastMessageId,
                    Type = x1.Type
                });

            if (t.HasData())
            {
                var sessionIds = t.Select(x => x.SessionId).ToArrayEx();

                var mappingIDs = await FreeSql.Select<im_chat_userInSession>()
                    .Where(x => sessionIds.Contains(x.SessionId)).ToListAsync(x => new {x.SessionId, x.UserId});

                foreach (var session in t)
                {
                    session.SessionUserIds = mappingIDs.Where(x => x.SessionId == session.SessionId)
                        .Select(x => x.UserId);
                }
                //FreeSql.Select<im_chat_userMessageStatus,im_chat_message>()
                //    .InnerJoin((x1,x2)=>x1.SessionId==x2.SessionId)
                //    .Where((x1,x2)=>x1.UserId==userId)
                //    .GroupBy((x1,x2)=>x2.SessionId) 
                //    .Having((x1,x2)=> x1.Value.Item2.MessageId> x1.Value.Item1.LastReadMessageId)

            }

            return new VM_PagedList<DTO_UserSession>(t,pageIndex,pageSize,(int)count);
        }

        /// <summary>
        /// 获取用户的所有未读数量
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns></returns>
        public async Task<IReadOnlyList<DTO_UserSessionUnReadCount>> GetUserSessionUnReadCount(string userId)
        {
            var sql = @"select m.SessionId,count(1) as UnReadCount
                    from
                    (
	                    select us.SessionId,isnull(s.LastReadMessageId,0) as LastReadMessageId
	                    from im_chat_session s1
	                    inner join im_chat_userInSession us on s1.SessionId=us.SessionId
	                    left join im_chat_userMessageStatus s on us.SessionId=s.SessionId
	                    where us.UserId=@userId
                    ) m
                    join im_chat_message m1 on m.SessionId=m1.SessionId and m1.MessageId>m.LastReadMessageId
                    group by m.SessionId
                    ";

            var lst = await FreeSql.Ado.QueryAsync<DTO_UserSessionUnReadCount>(sql, new {userId = userId});

            return lst;
        }

        /// <summary>
        /// 指定用户Id加入session会话中
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public async Task<ResultReturn> JoinToSession(string userId, long sessionId)
        {
            if (!await FreeSql.Select<im_chat_session>().Where(x=>x.SessionId==sessionId && x.Type== SessionTypeEnum.Group).AnyAsync())
            {
                return new FailResultReturn("指定会话不是群组会话,无法加入");
            }

            if (await FreeSql.Select<im_chat_userInSession>().AnyAsync(x=>x.SessionId==sessionId && x.UserId==userId))
            {
                return SuccessResultReturn.Default;
            }

            await FreeSql.Insert<im_chat_userInSession>(new im_chat_userInSession()
            {
                JoinDt = DateTime.Now,
                SessionId = sessionId,
                UserId = userId
            }).ExecuteInsertedAsync();

            return SuccessResultReturn.Default;
        }

        /// <summary>
        /// 获取两个用户的私聊会话id
        /// </summary>
        /// <param name="fromUserId"></param>
        /// <param name="toUserId"></param>
        /// <returns></returns>
        public async Task<long> GetToUseIdSessionIdByFromUseId(string fromUserId, string toUserId)
        {
            return await FreeSql.Select<im_chat_session>()
                .Where(x => x.ExtData.Contains($"{fromUserId},") && x.ExtData.Contains($"{toUserId},"))
                .ToOneAsync(x => x.SessionId);
        }

        /// <summary>
        /// 获取指定会话的用户Id列表
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<string>> GetSessionUserIds(long sessionId)
        {
            return await FreeSql.Select<im_chat_userInSession>().Where(x => x.SessionId == sessionId && x.QuitDt==null)
                .ToListAsync(x => x.UserId);
        }

        /// <summary>
        /// 获取指定Id会话信息
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public async Task<im_chat_session> GetSessionById(long sessionId)
        {
            return await FreeSql.Select<im_chat_session>().Where(x => x.SessionId == sessionId).ToOneAsync();
        }

        /// <summary>
        /// 获取指定用户的群组Id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<long>> GetUserGroupIDs(string userId)
        {
            return await FreeSql.Select<im_chat_userInSession>().Where(x => x.UserId == userId && x.QuitDt == null)
                .ToListAsync(x => x.SessionId);
        }

        /// <summary>
        /// 获取包含用户Id列表的群组信息
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public async Task<DTO_UserSession> GetSessionInfoById(long sessionId)
        {
            var s=await FreeSql.Select<im_chat_session>().Where(x => x.SessionId == sessionId).ToOneAsync<DTO_UserSession>();

            s.SessionUserIds = await GetSessionUserIds(sessionId);

            return s;
        }

        /// <summary>
        /// 判断指定用户是否在指定会话中
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<bool> IsUserExistsInSession(long sessionId, string userId)
        {
            return await FreeSql.Select<im_chat_userInSession>()
                .Where(x => x.SessionId == sessionId && x.UserId == userId).AnyAsync();
        }
    }
}
