using System;
using FreeSql;

namespace Kugar.IM.Services
{
    public abstract class BaseService
    {
        protected BaseService(IFreeSql freeSql)
        {
            FreeSql = freeSql;
        }

        protected IFreeSql FreeSql {  get; }
        
    }
}
