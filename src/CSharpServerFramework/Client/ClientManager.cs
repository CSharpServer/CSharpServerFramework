#define DEV_DEBUG
using CSharpServerFramework.Server;
using CSharpServerFramework.Util;
using CSharpServerFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CSharpServerFramework.Client
{
    /// <summary>
    /// 连接客户端管理器
    /// </summary>
    public class ClientManager
    {
        protected ClientAcceptQueue AcceptQueue { get; private set; }
        protected IDictionary<ulong,CSServerClientBase> OnlineClientMap { get; set; }
        protected object OnlineClientMapLock = new object();
        protected CSServer Server { get { return CSServer.InternalInstance; } }
        private Thread _initNewClientThread;
        private ManualResetEventSlim _tcpClientPushed;
        private ulong _clientIdSeed = 0;
        internal event EventHandler<ClientManagerEventArgs> OnClientDisconnectEvent;
        /// <summary>
        /// 用户连接的客户端Socket管理
        /// </summary>
        /// <param name="UserFactory">默认自定义用户实例的工厂</param>
        public ClientManager()
        {
            ManagerInit();
        }

        private void ManagerInit()
        {
            _tcpClientPushed = new ManualResetEventSlim(false);
            AcceptQueue = new ClientAcceptQueue();
            OnlineClientMap = new Dictionary<ulong, CSServerClientBase>();
            _initNewClientThread = new Thread(DoInitNewClient);
        }
        /// <summary>
        /// 初始化管理器
        /// </summary>
        public void Init()
        {
            try
            {
                _initNewClientThread.Start();
                Server.BaseRunningThreadCount++;
            }
            catch (Exception ex)
            {
                string errMsg = "Client Manager Init Faild:" + ex.Message;
                Server.Logger.Log(errMsg);
                throw new CSServerException(errMsg);
            }
            Server.Logger.Log("Client Manager Inited");
        }
        /// <summary>
        /// 停止管理器
        /// </summary>
        public void Stop()
        {
            while (!AcceptQueue.IsEmpty) ;
            _tcpClientPushed.Set();
            foreach (var item in OnlineClientMap)
            {
                item.Value.Dispose();
            }
        }
        /// <summary>
        /// 添加新Accept的Client
        /// </summary>
        /// <param name="Client"></param>
        public void PushClient(TcpClient Client)
        {
            this.AcceptQueue.PushClient(Client);
            _tcpClientPushed.Set();
        }
        private void DoInitNewClient(object obj)
        {
            uint validateTimeout = Server.ServerConfig.GetValidateTimeout();
            while(Server.ServerState == CSServer.CSServerState.RUNNING)
            {
                while(!AcceptQueue.IsEmpty)
                {
                    //这里可以考虑用异步?
                    try
                    {
                        TcpClient client = AcceptQueue.Dequeue();
                        UserSession session = new UserSession();
                        session.WorkThreadId = Server.WorkThreadManager.GetUserWorkThread();
                        CSServerClientBase csClient = new CSServerClientBase(client, GetNextClientId(), session, validateTimeout);
                        if (Server.MessageManager.RegistClient(csClient))
                        {
                            AddClient(csClient);
                        }
                    }
                    catch (Exception ex)
                    {
                        Server.Logger.Log("Init New Client Exception:" + ex);
                    }
                    
                }
                try
                {
                    //当前线程如果在这里停止，其他线程调用了PushClient并Set了_tcpClientPushed
                    //那么AcceptQueue非空，这里_tcpClientPushed.Wait()会阻塞线程，直到下一次
                    //_tcpClientPushed.Set()到来。所以这里使用Wait(TimeOut)，每隔一段时间解除线程阻塞
                    _tcpClientPushed.Reset();
                    _tcpClientPushed.Wait(5 * 1000);//5秒检测一次
                }
                catch (Exception ex)
                {
                    Server.Logger.Log("Init New Client Exception:" + ex);
                }
                
            }
        }

        /// <summary>
        /// 检测客户端是否已经断开
        /// </summary>
        /// <param name="Client">要检测的客户端</param>
        /// <returns>断开为ture</returns>
        public bool CheckClientIsDisconnected(CSServerClientBase Client)
        {
            RemoveClient(Client.Id);
            return true;
        }

        /// <summary>
        /// 判断用户是否已经离线
        /// </summary>
        /// <param name="User">用户基类</param>
        /// <returns>断开为ture</returns>
        public bool CheckSessionIsDisconneted(UserSession Session)
        {
            return CheckClientIsDisconnected(Session.Client);
        }

        protected ulong GetNextClientId()
        {
            return ++_clientIdSeed;
        }

        protected void AddClient(CSServerClientBase Client)
        {
            lock (this.OnlineClientMapLock)
            {
                OnlineClientMap.Add(Client.Id, Client);
            }
        }

        public bool DisconnectSession(UserSession Session)
        {
            return RemoveClient(Session.Client.Id);
        }

        protected bool RemoveClient(ulong ClientId)
        {
            CSServerClientBase client;

            lock (this.OnlineClientMapLock)
            {
                try
                {
                    client = OnlineClientMap[ClientId];
                    if (client == null)
                    {
                        return true;
                    }
                    OnlineClientMap.Remove(ClientId);
                }
                catch (Exception)
                {
                    return false;
                }
            }            
            Server.BufferManager.FreeBuffer(client.ReceiveBuffer.Id);
            ClientManagerEventArgs args = new ClientManagerEventArgs()
            {
                Session = client.Session,
                State = client
            };
            try
            {
                EventDispatcherUtil.AsyncDispatcherEvent(OnClientDisconnectEvent, this, args);
                client.Dispose();
#if DEV_DEBUG
                Server.Logger.Log("Client Disconnected");
#endif
                return true;
            }
            catch (Exception ex)
            {
                Server.Logger.Log("Client Manager.OnClientDisconnectEvent:" + ex);
                return false;
            }

        }

    }
}
