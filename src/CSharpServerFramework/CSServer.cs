using CSharpServerFramework.Buffer;
using CSharpServerFramework.Client;
using CSharpServerFramework.Extension;
using CSharpServerFramework.Log;
using CSharpServerFramework.Message;
using CSharpServerFramework.Server;
using CSharpServerFramework.WorkThread;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CSharpServerFramework.Util;
using System.Threading.Tasks;

namespace CSharpServerFramework
{
    /// <summary>
    /// 一个并发的服务器框架
    /// 使用Tcp协议通讯，所有Socket传输通过异步调用实现
    /// 通讯的数据协议可以自定义
    /// 服务端可以拓展多个Extension，不同的Extension处理服务端接收的不同数据
    /// 客户端发送过来的数据必须包括指定的Extension的处理方法，Extension通过反射机制将相关的数据解析后传递给相关的Extension的相关方法进行处理
    /// 服务端在接收和发送会处理数据包的包头，包头的定义是一个整形，代表包的长度（不包括包头数据长度），接收数据时会先接收包头再接收数据包，发送数据会先将数据包插入一个包头再发送
    /// 服务端配置数据在静态类CSServerBaseDefine,里面的数据是静态变量，可修改后再启动服务器
    /// </summary>
    public class CSServer : ICSServerBuilder
    {

        internal static CSServer InternalInstance { get; private set; }

        /// <summary>
        /// 服务端状态
        /// READY:服务端初始化完成，等待调用StartServer
        /// RUNNING:服务端运行中
        /// STOP:服务端停止
        /// </summary>
        public enum CSServerState
        {
            READY, RUNNING, STOP
        }
        protected TcpListener CSServerListener { get; private set; }
        protected int ServerPort { get; set; }
        protected IPAddress ServerBindIP { get; set; }

        public volatile CSServerState ServerState;
        public IGetServerConfig ServerConfig { get; private set; }
        protected int MaxListenConnection { get; set; }
        protected Thread CSServerListenThread { get; private set; }

        internal WorkThreadManager WorkThreadManager { get; private set; }
        internal CSServerLogger Logger { get; private set; }
        internal ClientManager ClientManager { get; private set; }
        internal BufferManager BufferManager { get; private set; }
        internal MessageManager MessageManager { get; private set; }
        internal ExtensionManager ExtensionManager { get; private set; }
        public int RunningThreadCount { get { return BaseRunningThreadCount + (WorkThreadManager == null ? 0 : WorkThreadManager.RunningWorkerThreadCount); } }
        public int BaseRunningThreadCount { get; internal set; }
        public event EventHandler OnServerStarted;
        public event EventHandler OnServerStoped;
        public event EventHandler<CSServerEventArgs> OnSessionDisconnected;

        /// <summary>
        /// 构造CSServer
        /// </summary>
        /// <param name="ExtensionLoaderBuilder">ExtensionLoader构造器</param>
        /// <param name="LoggerLoader">Logger加载器</param>
        /// <param name="ReceiveMessageFilter">自定义消息过滤器</param>
        /// <param name="UserFactory">自定义用户工厂</param>
        /// <param name="NetConfig">网络配置</param>
        /// <param name="BaseConfig">服务器基础配置</param>
        public CSServer()
        {
            ComponentInit();
            InitCSServer();
        }

        public void UseServerConfig(IGetServerConfig ServerConfig)
        {
            if (ServerConfig == null)
            {
                throw new CSServerException("Server Config Can't Be Null");
            }
            CSServerBaseDefine.BUFFER_SIZE = ServerConfig.GetBufferSize();
            CSServerBaseDefine.TIME_OUT = ServerConfig.GetNetTimeOut();
            CSServerBaseDefine.WORKER_THREAD_MAX = ServerConfig.GetWorkerThreadCount();
            CSServerBaseDefine.DEFAULT_BUFFER_LIST_SIZE = ServerConfig.GetBufferInitCount();
            this.ServerConfig = ServerConfig;
        }

        public void UseNetConfig(IGetNetConfig NetConfig)
        {
            if (NetConfig == null)
            {
                throw new CSServerException("Net Config Can't Be Null");
            }
            MaxListenConnection = NetConfig.GetMaxListenConnection();
            ServerBindIP = NetConfig.GetServerBindIP();
            ServerPort = NetConfig.GetListenPort();
        }

        private void InitCSServer()
        {
            ServerInit();
            ServerState = CSServerState.READY;
        }

        private void ComponentInit()
        {
            InternalInstance = this;
            BaseRunningThreadCount = 0;
            WorkThreadManager = new WorkThreadManager();
            Logger = new CSServerLogger();
            BufferManager = new BufferManager();
            ClientManager = new ClientManager();
            MessageManager = new MessageManager();
            ExtensionManager = new ExtensionManager();
        }

        /// <summary>
        /// 构造CSServer会调用
        /// </summary>
        protected virtual void ServerInit()
        {
        }

        /// <summary>
        /// StopServer调用后调用
        /// </summary>
        protected virtual void ServerDispose()
        {

        }

        /// <summary>
        /// 启动服务器
        /// </summary>
        public void StartServer()
        {
            if (ServerState != CSServerState.READY)
            {
                Logger.Log("Server is not ready");
                return;
            }
            try
            {
                ServerState = CSServerState.RUNNING;
                Logger.Init();
                BaseRunningThreadCount++;//log使用一条线程
                BufferManager.Init();
                ClientManager.Init();
                MessageManager.Init();
                ExtensionManager.Init();
                ClientManager.OnClientDisconnectEvent += ClientManager_OnClientDisconnectEvent;
                CSServerListenThread = new Thread(DoAcceptProc);
                CSServerListener = new TcpListener(ServerBindIP, ServerPort);
                CSServerListener.Start(MaxListenConnection);
                CSServerListenThread.Start();
                BaseRunningThreadCount++;
                WorkThreadManager.Init();
                Logger.Log(string.Format("Server Start,Listening {0}:{1}", ServerBindIP.ToString(), ServerPort));
                AfterStartServerInit();
                if (OnServerStarted != null)
                {
                    OnServerStarted.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
                this.StopServer();
                throw new CSServerException("Server Start Failed");
            }

        }

        private void ClientManager_OnClientDisconnectEvent(object sender, ClientManagerEventArgs e)
        {
            try
            {
                EventDispatcherUtil.DispatcherEvent(this.OnSessionDisconnected, this, new CSServerEventArgs { State = e.Session });
            }
            catch (Exception ex)
            {
                Logger.Log("On Client Disconnect Exception:" + ex);
            }
            
        }

        /// <summary>
        /// 调用StartServer后调用
        /// </summary>
        protected virtual void AfterStartServerInit()
        {

        }

        /// <summary>
        /// 启动服务器
        /// </summary>
        /// <param name="IP">绑定的IP</param>
        /// <param name="Port">监听的端口</param>
        public void StartServer(IPAddress IP, int Port)
        {
            this.ServerBindIP = IP;
            this.ServerPort = Port;
            StartServer();
        }

        /// <summary>
        /// 停止服务器
        /// </summary>
        public void StopServer()
        {
            ClientManager.OnClientDisconnectEvent -= ClientManager_OnClientDisconnectEvent;
            ServerState = CSServerState.STOP;
            ClientManager.Stop();
            MessageManager.Stop();
            WorkThreadManager.Stop();
            BufferManager.Dispose();
            Logger.Log("Server Stopped");
            Logger.Stop();
            ServerDispose();
            if (OnServerStoped != null)
            {
                OnServerStoped.Invoke(this, EventArgs.Empty);
            }
        }

        private void DoAcceptProc(object obj)
        {
            try
            {
                Task.Run(async () =>
                {
                    while (ServerState == CSServerState.RUNNING)
                    {
                        var newClient = await CSServerListener.AcceptTcpClientAsync();
                        ClientManager.PushClient(newClient);
                    }
                    Logger.Log("Server Listen Thread Stop");
                });
            }
            catch (Exception ex)
            {
                Logger.Log("Server Exception:" + ex);
            }
        }

        public void UseExtension(ICSharpServerExtension extension)
        {
            ExtensionManager.RegistExtension(extension);
        }

        public void UseLogger(ILoggerLog Logger)
        {
            this.Logger.AddLogger(Logger);
        }

        public void UseMessageRoute(IGetMessageRoute Router)
        {
            MessageManager.AddFilter(Router);
        }
    }
}