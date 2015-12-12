using CSharpServerFramework.Buffer;
using CSharpServerFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace CSharpServerFramework.Client
{
    public class CSServerClientBase
    {
        /// <summary>
        /// CSServer管理的Client
        /// 封装了TcpClient
        /// </summary>
        /// <param name="Client">Accept的TcpClient</param>
        /// <param name="Id">分配给该Client的Id</param>
        /// <param name="UserInfo">用户信息</param>
        /// <param name="NotValidateRecvTimeOut">认证超时，在次时间内没有发认证信息断开连接，释放资源</param>
        public CSServerClientBase(TcpClient Client,ulong Id,UserSession Session, uint NotValidateRecvTimeOut)
        {
            BaseTcpClient = Client;
            this.Session = Session;
            this.Session.Client = this;
            this.Id = Id;
            this.NotValidateRecvTimeOut = NotValidateRecvTimeOut;
        }
        public UserSession Session { get; private set; }
        public ulong Id { get; private set; }
        public TcpClient BaseTcpClient { get; private set; }
        public uint NotValidateRecvTimeOut { get; private set; }

        internal CSServerBuffer ReceiveBuffer { get; set; }

        public void Dispose()
        {
            ReceiveBuffer = null;
			BaseTcpClient.Client.Dispose ();
            BaseTcpClient.Close();
        }
    }
}
