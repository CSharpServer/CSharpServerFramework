using CSharpServerFramework.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpServerFramework.Message;
using CSharpServerFramework;

namespace CSharpServerFramework.Extension.ServerBaseExtensions
{
    public interface HandleUnknowCommand
    {
        /// <summary>
        /// 处理客户端发送的未知的消息
        /// </summary>
        /// <param name="UnknowExtensionName">未知的Extension名</param>
        /// <param name="UnknowCommandId">未知的CommandId</param>
        /// <param name="SendMessageUser">发送的用户</param>
        /// <param name="Data">发送的数据流</param>
        /// <param name="DataLength">数据流长度</param>
        void HandleUnknowCommand(string UnknowExtensionName, int UnknowCommandId, UserSession SendMessageUser, byte[] Data, int DataLength);
    }
    /// <summary>
    /// 未知消息处理Extension
    /// 继承此类，并用IExtensionLoader加载进CSServer可以自定义处理未知消息，否则CSServer以异常的方式抛出未知的命令的信息并用Logger记录
    /// 子类需要指定类的ExtensionInfo特征属性
    /// </summary>
    public class UnknowCommandExtensionBase : ExtensionBase
    {
        public HandleUnknowCommand UnknowCommandHandler { get; private set; }
        public UnknowCommandExtensionBase(HandleUnknowCommand Handler):base(null)
        {
            UnknowCommandHandler = Handler;
        }

        internal override ExtensionCommand GetCommand(ReceiveMessage Message)
        {
            return null;
        }

        internal override void HandleMessage(ExtensionCommand Command, Message.ReceiveMessage Message,UserSession Session)
        {
            try
            {
                UnknowCommandHandler.HandleUnknowCommand
                (Message.ExtensionName, Message.CommandId, Session, Message.ReceiveDataBuffer.AllBuffer, Message.ReceiveDataBuffer.BufferTotalLength);
            }
            catch (Exception ex)
            {
                throw new ExtensionException("Command Method Invoke Exception:" + ex.Message);
            }
        }

    }
}
