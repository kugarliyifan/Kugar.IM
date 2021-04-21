using System;
using System.Collections.Generic;
using System.Text;
using Kugar.IM.DB.Entities;

namespace Kugar.IM.DB.DTO
{
    public class DTO_UserSession:im_chat_session
    {
        public IReadOnlyList<string> SessionUserIds { set;get; }
    }
}
