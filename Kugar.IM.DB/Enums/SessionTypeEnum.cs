using System;
using System.Collections.Generic;
using System.Text;

namespace Kugar.IM.DB.Enums
{
    /// <summary>
    /// 会话类型枚举
    /// </summary>
    public enum SessionTypeEnum
    {
        /// <summary>
        /// 个人私聊
        /// </summary>
        OneToOne=0,

        /// <summary>
        /// 群组
        /// </summary>
        Group=1,

        /// <summary>
        /// 系统会话
        /// </summary>
        System=2
    }

    /// <summary>
    /// 消息类型
    /// </summary>
    public enum MessageTypeEnum
    {
        /// <summary>
        /// 系统消息,系统消息无发送人
        /// </summary>
        System,

        /// <summary>
        /// 用户消息
        /// </summary>
        User
    }

    /// <summary>
    /// 消息内容类型
    /// </summary>
    public enum MessageContentTypeEnum
    {
        /// <summary>
        /// 文本
        /// </summary>
        Text=0,

        /// <summary>
        /// 图片
        /// </summary>
        Image=1,

        /// <summary>
        /// 语音
        /// </summary>
        Voice=2,

        /// <summary>
        /// 小视频
        /// </summary>
        Video=3,

        /// <summary>
        /// 表情
        /// </summary>
        Emoticon=4
    }
}
