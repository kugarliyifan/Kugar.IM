using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.Web.JsonTemplate.Builders;
using Kugar.Core.Web.JsonTemplate.Helpers;
using Kugar.Core.Web.JsonTemplate.Templates;
using Kugar.IM.DB.DTO;

namespace Kugar.IM.Server.Areas.IM.Views.Session
{
    public class View_GetSessions:WrapResultReturnJsonTemplateBase<IPagedList<DTO_UserSession>>
    {
        protected override void BuildReturnDataScheme(IChildObjectBuilder<IPagedList<DTO_UserSession>> builder)
        {
            using (var b=builder.FromPagedList(x=>x.Model))
            {
                b.AddProperties(x => x.HeaderImageUrl, 
                    x => x.CreateDt, 
                    x => x.SessionId, 
                    x => x.Name,
                    x => x.LastMessageId, 
                    x => x.LastActivityDt, 
                    x => x.SessionUserIds, 
                    x => x.Type);
            }
        }
    }
}
