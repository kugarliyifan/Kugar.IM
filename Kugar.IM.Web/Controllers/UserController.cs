using System.Threading.Tasks;
using Kugar.Core.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kugar.IM.Web.Controllers
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
