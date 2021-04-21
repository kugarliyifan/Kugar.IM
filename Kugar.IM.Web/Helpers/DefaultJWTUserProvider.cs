using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.SignalR;

namespace Kugar.IM.Web.Helpers
{
    public class DefaultJWTUserProvider:IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
