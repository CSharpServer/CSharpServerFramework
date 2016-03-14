#define DEV_DEBUGN
#define DEV_DEBUG_ASYNCN
using CSharpServerFramework.Client;
using CSharpServerFramework.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSharpServerFramework.Extension
{
    /// <summary>
    /// Extension基类
    /// </summary>
    public class ExtensionBase : ICSharpServerExtension,IExtensionMessageRedirect,IExtensionLog, IExtensionSessionManage
    {
        public ExtensionBaseEx Extend { get; set; }
        public string ExtensionName { get; private set; }
        public IDeserializeMessage MessageDecompressor { get; private set; }
        internal ExtensionManager ExtensionManagerInstance { get; set; }
        protected IEnumerable<ExtensionCommand> Commands { get; set; }

        /// <summary>
        /// 处理消息委托
        /// </summary>
        /// <param name="User">用户</param>
        /// <param name="Object">参数</param>
        protected delegate void ExtensionHandleMessageDelegate(ExtensionBase Extension, ICSharpServerSession Session, object Object, MethodInfo Method);

        /// <summary>
        /// 异步处理消息方法
        /// </summary>
        /// <param name="Extension"></param>
        /// <param name="Session"></param>
        /// <param name="Obj"></param>
        /// <param name="Method"></param>
        protected static void AsyncMessageHandler(ExtensionBase Extension, ICSharpServerSession Session, object Obj, MethodInfo Method)
        {
#if DEV_DEBUG_ASYNC
            Console.WriteLine("Current Thread:" + System.Threading.Thread.CurrentThread.GetHashCode());
#endif
            try
            {
                Method.Invoke(Extension.Extend, new object[] { Session, Obj });
            }
            catch (Exception ex)
            {
                Extension.Log("Async Invoke Method Exception:" + ex.Message);
            }
            
        }
        /// <summary>
        /// 异步调用回调函数
        /// </summary>
        /// <param name="ar"></param>
        protected static void AsyncMessageHandlerCallback(IAsyncResult ar)
        {
            try
            {
                ExtensionHandleMessageDelegate handler = ar.AsyncState as ExtensionHandleMessageDelegate;
                handler.EndInvoke(ar);
            }
            catch (Exception ex)
            {
                CSServer.InternalInstance.Logger.Log("Async Handle Message Exception:" + ex);
            }
            
#if DEV_DEBUG_ASYNC
            Console.WriteLine("Current Thread:" + System.Threading.Thread.CurrentThread.GetHashCode());
#endif
        }

        /// <summary>
        /// CSharpServer 的Extension基类
        /// </summary>
        ///<param name="Extend">拓展代理</param>
        public ExtensionBase(ExtensionBaseEx Extend)
        {
            this.Extend = Extend;
            this.Extend.ServerExtensionBase = this;
            ExtensionName = Extend.ExtensionName;
            MessageDecompressor = Extend.MessageDecompressor;
            Commands = Extend.LoadCommand();
        }

        protected object DeserializeMessage(ReceiveMessage Message)
        {
            object data;
            try
            {
                if (Message.MessageObject == null)
                {
                    data = MessageDecompressor.DeserializeMessage(Message.CommandId, Message.ReceiveDataBuffer.AllBuffer, Message.ReceiveDataBuffer.BufferTotalLength);
                    //解压完消息后释放缓存
                    ExtensionManagerInstance.Server.BufferManager.FreeBuffer(Message.ReceiveDataBuffer.Id);
                    return data;
                }
                else
                {
                    return Message.MessageObject;
                }

            }
            catch (Exception e)
            {
                throw new ExtensionException("Decompress Message Exception:" + e.Message);
            }
        }

        internal virtual ExtensionCommand GetCommand(ReceiveMessage Message)
        {
            if (Message.CommandId == -1)
            {
                return GetCommand(Message.CommandName);
            }
            return GetCommand(Message.CommandId);
        }

        protected virtual ExtensionCommand GetCommand(string CommandName)
        {
            ///实际上一个Extension的Command不多，所以直接遍历吧
            foreach (var item in Commands)
            {
                if (item.CommandName == CommandName)
                {
                    return item;
                }
            }
            throw new ExtensionException(string.Format("Extension<{0}> No Command<{1}>", ExtensionName, CommandName));
        }

        protected virtual ExtensionCommand GetCommand(int CommandId)
        {
            ///实际上一个Extension的Command不多，所以直接遍历吧
            foreach (var item in Commands)
            {
                if (item.CommandId == CommandId)
                {
                    return item;
                }
            }
            throw new ExtensionException(string.Format("Extension<{0}> No Command<{1}>", ExtensionName, CommandId));
        }

        public bool RedirectMessage(string ExtensionName, int CommandId, ICSharpServerSession Session, dynamic msg)
        {
            var reMsg = new ReceiveMessage()
            {
                CommandId = CommandId,
                ExtensionName = ExtensionName,
                MessageObject = msg
            };
            try
            {
                ExtensionManagerInstance.RedirectReceiveMessage(reMsg, Session as UserSession);
                return true;
            }
            catch (Exception ex)
            {
                Log("Redirect Message:" + ex);
                return false;
            }
            
        }

        public bool RedirectMessage(string ExtensionName, string CommandName, ICSharpServerSession Session, dynamic msg)
        {
            var reMsg = new ReceiveMessage()
            {
                CommandName = CommandName,
                ExtensionName = ExtensionName,
                MessageObject = msg
            };
            try
            {
                ExtensionManagerInstance.RedirectReceiveMessage(reMsg, Session as UserSession);
                return true;
            }
            catch (Exception ex)
            {
                Log("Redirect Message:" + ex);
                return false;
            }
            
        }

        internal virtual void HandleMessage(ExtensionCommand Command, ReceiveMessage Message, UserSession Session)
        {

#if DEV_DEBUG
            Log("HandleMessage Invoke:" + (DateTime.UtcNow.Ticks - CSServerBaseDefine.ReceiveMessageTick));
#endif

#if DEV_DEBUG_ASYNC
            Console.WriteLine("Current Thread:" + System.Threading.Thread.CurrentThread.GetHashCode());
#endif

#if DEV_DEBUG
                    Log("HandleMessage Start Interval:" + (DateTime.UtcNow.Ticks - CSServerBaseDefine.ReceiveMessageTick));
#endif
            try
            {
                object data = null;
                if (Command.IsAcceptRawDataCommand)
                {
                    data = Message.ReceiveDataBuffer.AllBuffer.Clone();
                }
                else
                {
                    data = DeserializeMessage(Message);
                }

                if (Command.IsAsyncInvoke)
                {
                    //异步方式调用
                    ExtensionHandleMessageDelegate handler = new ExtensionHandleMessageDelegate(AsyncMessageHandler);
                    handler.BeginInvoke(this, Session, data, Command.CommandMethod, AsyncMessageHandlerCallback, handler);
                }
                else
                {
                    //同步方式调用
                    Command.CommandMethod.Invoke(this, new object[] { Session, data });
                }

            }
            catch (Exception ex)
            {

                throw new ExtensionException("Command Method Invoke Exception:" + ex.Message);
            }
#if DEV_DEBUG
                    Log("HandleMessage End Interval:" + (DateTime.UtcNow.Ticks - CSServerBaseDefine.ReceiveMessageTick));
#endif
        }
        /// <summary>
        /// 发送消息给单个用户
        /// </summary>
        /// <param name="UserInfo">消息接收的用户</param>
        /// <param name="Message">消息体</param>
        public void SendResponse(ICSharpServerSession Session, SendMessage Message)
        {
            var message = new SendUserMessage(Message.DataBuffer, Message.BufferLength);
            message.Client = (Session as UserSession).Client;
            ExtensionManagerInstance.RedirectSendMessage(message);
        }
        /// <summary>
        /// 发送消息给多个用户
        /// </summary>
        /// <param name="Message">消息体</param>
        public void SendResponseToUsers(IEnumerable<ICSharpServerUser> Users, SendMessage Message)
        {
            var message = new SendUsersMessage(Message.DataBuffer, Message.BufferLength);
            message.Users = Users.ToArray();
            ExtensionManagerInstance.RedirectSendMessage(message);
        }
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="LogMessage">日志内容</param>
        public void Log(string LogMessage)
        {
            ExtensionManagerInstance.Log(string.Format("Extension(\"{0}\"):{1}", ExtensionName, LogMessage));
        }

        internal void Init()
        {
            Extend.Init();
        }

        public void CloseSession(ICSharpServerSession Session)
        {
            ExtensionManagerInstance.Server.ClientManager.DisconnectSession(Session as UserSession);
        }
    }
}
