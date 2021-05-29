using System;
using System.Collections.Generic;
using System.Text;
using Kugar.IM.DB.Enums;

namespace Kugar.IM.DB.DTO
{
    public class DTO_SessionMessage
    {
        public long MessageId { set; get; }

        /// <summary>
        /// 消息类型
        /// </summary>
        public MessageTypeEnum MessageType { set; get; }

        /// <summary>
        /// 发送方用户Id
        /// </summary>
        public string SendUserId { set; get; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content { set; get; }

        /// <summary>
        /// 消息内容类型
        /// </summary>
        public MessageContentTypeEnum ContentType { set; get; }

        /// <summary>
        /// 是否为当前用户发送的
        /// </summary>
        public bool IsMe { set; get; }

        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime SendDt { set; get;}

        /// <summary>
        /// 消息状态 0=显示 1=已撤回
        /// </summary>
        public int State { set; get; }
    }
}
