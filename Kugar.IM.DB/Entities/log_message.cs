using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using FreeSql.DataAnnotations;
using Kugar.IM.DB.Enums;

namespace Kugar.IM.DB.Entities
{
    /// <summary>
    /// 消息表
    /// </summary>
    [Index("{tablename}_idx_sessionid","SessionId desc")]
    [Index("{tablename}_idx_Type","Type desc")]
    [Index("{tablename}_idx_SenderId","SenderId desc")]
    [Index("{tablename}_idx_State","State asc")]
    [Index("{tablename}_idx_SendDt","SendDt asc")]
    public class im_chat_message
    {
        /// <summary>
        /// 消息id
        /// </summary>
        [FreeSql.DataAnnotations.Column(IsPrimary = true,IsIdentity = true)]
        public long MessageId { set; get; }

        /// <summary>
        /// 消息类型  0=系统消息  1=用户消息
        /// </summary>
        [FreeSql.DataAnnotations.Column( MapType= typeof(int))]
        public MessageTypeEnum Type { set; get; }

        /// <summary>
        /// 会话Id
        /// </summary>
        public long SessionId { set; get; }

        /// <summary>
        /// 消息内容,json格式,如果ContentType=0 则直接为文本内容
        /// </summary>
        [FreeSql.DataAnnotations.Column(StringLength = 400)]
        public string Content { set; get; }

        /// <summary>
        /// 消息类型   0=文本  1=图片 2=语音 3=小视频
        /// </summary>
        [FreeSql.DataAnnotations.Column( MapType= typeof(int))]
        public MessageContentTypeEnum ContentType { set; get; }
        
        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime SendDt { set; get; }

        /// <summary>
        /// 发送人Id,如果type=0 则为空
        /// </summary>
        public string SenderId { set; get; }
        
        /// <summary>
        /// 状态 0=正常 99=撤回
        /// </summary>
        public int State { set; get; }
    }

    /// <summary>
    /// 聊天会话
    /// </summary>
    [Index("{tablename}_idx_SessionId","SessionId desc")]
    [Index("{tablename}_idx_LastActivityDt","LastActivityDt desc")]
    public class im_chat_session
    {
        [FreeSql.DataAnnotations.Column(IsPrimary = true,IsIdentity = true)]
        public long SessionId { set; get; }

        /// <summary>
        /// 会话名称
        /// </summary>
        [FreeSql.DataAnnotations.Column(StringLength = 30)]
        public string Name { set; get; }

        /// <summary>
        /// 会话头像图片
        /// </summary>
        public string HeaderImageUrl { set; get; }

        /// <summary>
        /// 会话类型  0=个人会话 1=群组会话 2=系统会话
        /// </summary>
        [FreeSql.DataAnnotations.Column( MapType= typeof(int))]
        public SessionTypeEnum Type { set; get; }

        /// <summary>
        /// 是否为公开会话
        /// </summary>
        public bool IsPublic { set; get; }

        /// <summary>
        /// 创建者Id
        /// </summary>
        public string CreateUserId { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateDt { set; get; }

        /// <summary>
        /// 最后一次消息时间
        /// </summary>
        public DateTime? LastActivityDt { set; get; }

        /// <summary>
        /// 最后一条消息Id
        /// </summary>
        public long? LastMessageId { set; get; }

        /// <summary>
        /// 扩展信息
        /// </summary>
        [FreeSql.DataAnnotations.Column(IsPrimary = false ,IsNullable = true)]
        public string ExtData { set; get; }

        
    }

    /// <summary>
    /// 用户消息已读状态
    /// </summary>
    public class im_chat_userMessageStatus
    {
        [FreeSql.DataAnnotations.Column(IsPrimary = true )]
        public long SessionId { set; get; }


        [FreeSql.DataAnnotations.Column(IsPrimary = true )]
        public string UserId { set; get; }
        
        /// <summary>
        /// 该会话中,最后一次读取的消息ID
        /// </summary>
        public long LastReadMessageId { set; get; }
        
    }

    /// <summary>
    /// 聊天会话用户关联表
    /// </summary>
    public class im_chat_userInSession 
    {
        /// <summary>
        /// 会话Id
        /// </summary>
        [FreeSql.DataAnnotations.Column(IsPrimary = true)]
        public long SessionId { set; get; }

        /// <summary>
        /// 会员Id
        /// </summary>
        [FreeSql.DataAnnotations.Column(IsPrimary = true)]
        public string UserId { set; get; }

        /// <summary>
        /// 会话顺序,数字越小,越靠前
        /// </summary>
        public int Order { set; get; }

        /// <summary>
        /// 加入会话时间
        /// </summary>
        public DateTime JoinDt { set; get; }

        /// <summary>
        /// 退出会话时间
        /// </summary>
        public DateTime? QuitDt { set; get; }
    }

    [FreeSql.DataAnnotations.Table(Name = "im_chat_userConnection")]
    public class im_chat_userconnection
    {
        [FreeSql.DataAnnotations.Column(IsPrimary = true)]
        public string UserId { set; get; }

        [FreeSql.DataAnnotations.Column(IsPrimary = true)]
        public string ConnectionId { set; get; }

        public DateTime LastDt { set; get; }
    }
    
}
