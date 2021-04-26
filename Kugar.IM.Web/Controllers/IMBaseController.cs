using System;
using System.Linq;
using System.Security.Claims;
using Kugar.Core.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Kugar.IM.Web.Controllers
{
    [Area("im")]
    [Authorize(AuthenticationSchemes = "im")]
    [FromBodyJson/*,conve(typeof(SomeControllerModelConvention))*/ ]
    public class IMBaseController : ControllerBase
    {
        protected string CurrentUserId => this.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }


    //public class SomeControllerModelConvention : Attribute, IControllerModelConvention
    //{
    //    public SomeControllerModelConvention()
    //    {

    //    }

    //    public void Apply(ControllerModel model)
    //    {
    //        var t = (AuthorizeAttribute) model.Attributes.FirstOrDefault(x => x is AuthorizeAttribute);

    //        t.AuthenticationSchemes = "im";

    //        //foreach (var actionModel in model.Actions)
    //        //    actionModel.Attributes.Add(new AuthorizeAttribute());
    //    }
    //}
}
