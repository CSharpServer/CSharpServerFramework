using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CSharpServerFramework.Client
{
    /// <summary>
    /// Accept的队列
    /// </summary>
    public class ClientAcceptQueue
    {
        //private ConcurrentQueue<TcpClient> _queue = new ConcurrentQueue<TcpClient>();
        private Queue<TcpClient> _queue = new Queue<TcpClient>();
        public ClientAcceptQueue()
        {
        }
        public void PushClient(TcpClient Client)
        {
            _queue.Enqueue(Client);
        }
        public TcpClient Dequeue()
        {
            return _queue.Dequeue();
        }
        public bool IsEmpty
        {
            get
            {
                return _queue.Count == 0;
            }
        }
    }
}
