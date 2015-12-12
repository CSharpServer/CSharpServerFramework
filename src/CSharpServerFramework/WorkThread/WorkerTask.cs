using CSharpServerFramework.Client;
using CSharpServerFramework.Extension;
using CSharpServerFramework.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpServerFramework.WorkThread
{
    /// <summary>
    /// 工作线程的任务
    /// </summary>
    public class WorkerTask
    {
        /// <summary>
        /// 处理本任务的Extension
        /// </summary>
        protected ExtensionBase Handler { get; private set; }
        /// <summary>
        /// 本任务的消息
        /// </summary>
        protected ReceiveMessage Message { get; private set; }
        /// <summary>
        /// 用户信息
        /// </summary>
        public UserSession Session { get; private set; }

        /// <summary>
        /// 顺序的任务，先请求的先响应
        /// </summary>
        public bool IsOrderTask { get; private set; }

        /// <summary>
        /// 处理Message的Command
        /// </summary>
        public ExtensionCommand Command { get; private set; }

        public WorkerTask(ExtensionBase Handler,ReceiveMessage Message,UserSession Session)
        {
            this.Command = Handler.GetCommand(Message);
            this.IsOrderTask = Command.isOrder;
            this.Handler = Handler;
            this.Message = Message;
            this.Session = Session;
        }
        /// <summary>
        /// 调用Extension的相应的方法处理消息
        /// </summary>
        public void DoWork()
        {
            Handler.HandleMessage(Command, Message, Session);            
        }
    }
}
