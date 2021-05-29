using System;
using System.Collections.Generic;
using System.Text;
using Kugar.IM.DB.Entities;
using Kugar.IM.DB.Enums;

namespace Kugar.IM.DB.DTO
{
    public class DTO_UserSession:im_chat_session
    {
        public IEnumerable<string> SessionUserIds { set;get; }

        public string LastMessageContent { set; get; }
        

        public int? ContentType { set; get; }

        public int unreadCount { set; get; }
    }
}
