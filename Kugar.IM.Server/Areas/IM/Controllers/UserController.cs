using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kugar.Core.Web.Controllers;
using Kugar.Core.Web.JsonTemplate.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace Kugar.IM.Server.Areas.IM.Controllers
{
    public class UserController : IMBaseController
    {
        [AllowAnonymous]
        public async Task<string> TestLogin([FromQuery]string userId)
        {
            var token = this.BuildJWtToken(userId, "ssss");

            return token;
        }


    }
}
