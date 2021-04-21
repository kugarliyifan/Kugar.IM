using System;
using System.Collections.Generic;
using System.Text;

namespace Kugar.IM.DB.DTO
{
    public class DTO_UserSessionUnReadCount
    {
        public long SessionId { set; get; }

        public int UnReadCount { set; get; }
    }
}
