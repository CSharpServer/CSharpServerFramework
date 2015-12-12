using CSharpServerFramework.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace CSharpServerFramework.Server
{
    /// <summary>
    /// 通讯异步调用的附带数据
    /// </summary>
    public class CommunicateAsyncState:CSServerAsyncState
    {
        public TcpClient Client { get { return (TcpClient)this.State; } }
        
        public CommunicateAsyncState(TcpClient Client, CSServerBuffer Buffer,AsyncCallback Callback):base(Client)
        {
            
        }
    }
}
