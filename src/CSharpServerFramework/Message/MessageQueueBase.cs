using CSharpServerFramework.Message;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpServerFramework.Message
{
    /// <summary>
    /// 消息队列基类
    /// </summary>
    public class MessageQueueBase
    {
        private ConcurrentQueue<ICSharpServerMessageData> _queue;
        public MessageQueueBase()
        {
            _queue = new ConcurrentQueue<ICSharpServerMessageData>();
        }
        public bool PushMessage(ICSharpServerMessageData Message)
        {
            _queue.Enqueue(Message);
            return true;
        }
        public ICSharpServerMessageData Peek()
        {
            ICSharpServerMessageData result;
            _queue.TryPeek(out result);
            return result;
        }

        public ICSharpServerMessageData Dequeue()
        {
            ICSharpServerMessageData result;
            _queue.TryDequeue(out result);
            return result;
        }

        public bool IsEmpty
        {
            get { return _queue.IsEmpty; }
        }
    }
}
