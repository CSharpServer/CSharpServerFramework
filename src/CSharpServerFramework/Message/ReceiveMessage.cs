using CSharpServerFramework.Client;
using CSharpServerFramework;
using CSharpServerFramework.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpServerFramework.Message
{
    /// <summary>
    /// 接收的数据
    /// </summary>
    public class ReceiveMessage:MessageBase
    {
        /// <summary>
        /// 获取或设置接收到的数据缓存
        /// </summary>
        public Buffer.CSServerBuffer ReceiveDataBuffer { get; set; }

        /// <summary>
        /// 反序列化后的消息实体
        /// </summary>
        public dynamic MessageObject { get; set; }
    }
}
