using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kugar.Core.ExtMethod;
using Kugar.IM.DB.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace Kugar.IM.Web.Cache
{
    public interface IIMConnectionManager
    {

        Task Init();

        Task<bool> IsUserOnline(string userId);

        Task<IEnumerable<string>> GetUserConnectionIds(string userId);

        Task AddUserConnectionId(string userId, string connectionId);

        Task RemoveUserConnectionId(string userId, string connectinoId);

        Task ActivityUser(string userId);
    }

    /// <summary>
    /// 
    /// </summary>
    public class DefaultMemoryUserConnectManager:IIMConnectionManager
    {
        private MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private IFreeSql _freeSql = null;

        public DefaultMemoryUserConnectManager(IFreeSql freesql)
        {
            _freeSql = freesql;
        }

        public async Task Init()
        {
            var users = await _freeSql.Select<im_chat_userconnection>()
                .Where(x =>  x.LastDt > DateTime.Now.AddMinutes(-3))
      
                .ToListAsync( );

            if (users.HasData())
            {
                foreach (var user  in users)
                {
                    _cache.Set(user.UserId, new HashSet<string>(users.Where(x=>x.UserId==user.UserId).Select(x=>x.ConnectionId)), new MemoryCacheEntryOptions()
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(2)
                    });
                }
            } 
        }

        public async Task<bool> IsUserOnline(string userId)
        {
            if (_cache.TryGetValue(userId,out HashSet<string> conns) && conns.Count>0)
            {
                return true;
            }
            else
            {
                var conn = await _freeSql.Select<im_chat_userconnection>()
                    .Where(x => x.UserId == userId && x.LastDt > DateTime.Now.AddMinutes(-3))
                    .ToListAsync(x => x.ConnectionId);

                if (conn.HasData())
                {
                    _cache.Set(userId, new HashSet<string>(conn), new MemoryCacheEntryOptions()
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(2)
                    });

                    return true;
                }

                return false ;
            }
        }

        public async Task<IEnumerable<string>> GetUserConnectionIds(string userId)
        {
            if (_cache.TryGetValue(userId,out HashSet<string> conn))
            {
                return conn;
            }
            else
            {
                var lst=await _freeSql.Select<im_chat_userconnection>()
                    .Where(x => x.UserId == userId && x.LastDt > DateTime.Now.AddMinutes(-3))
                    .ToListAsync(x => x.ConnectionId);

                if (lst.HasData())
                {
                    return lst;
                }

                return Array.Empty<string>();
            }
        }

        public async Task AddUserConnectionId(string userId, string connectionId)
        {
            
            if (_cache.TryGetValue(userId,out HashSet<string> tmp) )
            {
                tmp.Add(connectionId);
            }
            else
            {
                lock (userId)
                {
                    _cache.Set(userId, new HashSet<string>()
                    {
                        connectionId
                    },new MemoryCacheEntryOptions()
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(5)
                    });
                }
            }

            await _freeSql.InsertOrUpdate<im_chat_userconnection>().SetSource(new im_chat_userconnection()
            {
                LastDt = DateTime.Now,
                ConnectionId = connectionId,
                UserId = userId
            }).ExecuteAffrowsAsync();

        }

        public async Task RemoveUserConnectionId(string userId, string connectionId)
        {
            if (_cache.TryGetValue(userId,out HashSet<string> tmp) )
            {
                tmp.Remove(connectionId);

                await _freeSql.Delete<im_chat_userconnection>()
                    .Where(x => x.UserId == userId && x.ConnectionId == connectionId).ExecuteAffrowsAsync();
            }

            await _freeSql.Delete<im_chat_userconnection>().Where(x => x.LastDt < DateTime.Now.AddMinutes(-5))
                .ExecuteAffrowsAsync();
        }

        public async Task ActivityUser(string userId)
        {
            _cache.Get(userId);

            //await _freeSql.Delete<>()
        }
    }
}
