using CSharpServerFramework.Buffer;
using CSharpServerFramework.Client;
using CSharpServerFramework.Server;
using CSharpServerFramework.Util;
using CSharpServerFramework;
using CSharpServerFramework.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpServerFramework.Message
{

    public class SendUserMessage:SendMessage
    {
        /// <summary>
        /// 接收服务端发送信息的Client
        /// </summary>
        internal CSServerClientBase Client { get; set; }
        
        public SendUserMessage(byte[] Data)
        {
        }
        /// <summary>
        /// 构造SendMessage
        /// </summary>
        /// <param name="Data">数据包</param>
        /// <param name="Length">数据包长度</param>
        public SendUserMessage(byte[] Data,int Length)
        {
            byte[] msg = new byte[CSServerBaseDefine.TCP_PACKAGE_HEAD_SIZE + Length];
            ///封装包头
            BufferLength = BitUtil.CreateDataPackageWithHead(msg, Data, Length);
            DataBuffer = msg;
        }
    }

    /// <summary>
    /// 发送给多个用户的消息
    /// </summary>
    public class SendUsersMessage:SendMessage
    {
        internal IEnumerable<ICSharpServerUser> Users { get; set; }
        public SendUsersMessage(byte[] Data)
            : this(Data, Data.Length)
        {
        }
        /// <summary>
        /// 构造SendMessage
        /// </summary>
        /// <param name="Data">数据包</param>
        /// <param name="Length">数据包长度</param>
        public SendUsersMessage(byte[] Data, int Length)
        {
            byte[] msg = new byte[CSServerBaseDefine.TCP_PACKAGE_HEAD_SIZE + Length];
            ///封装包头
            this.BufferLength = BitUtil.CreateDataPackageWithHead(msg, Data, Length);
            this.DataBuffer = msg;
        }
    }
}
