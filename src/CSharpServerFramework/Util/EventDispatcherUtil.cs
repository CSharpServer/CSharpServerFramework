using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpServerFramework.Util
{
    public class EventDispatcherUtil
    {
        private static void DefaultEndCallback(IAsyncResult Ar)
        {
            EventHandler handler = Ar.AsyncState as EventHandler;
            if(handler != null)
            {
                handler.EndInvoke(Ar);
            }            
        }

        private static void DefaultEndCallbackArgs<TEventArgs>(IAsyncResult Ar) where TEventArgs : EventArgs
        {
            EventHandler<TEventArgs> handler = Ar.AsyncState as EventHandler<TEventArgs>;
            if (handler != null)
            {
                handler.EndInvoke(Ar);
            }
        }

        public static void DispatcherEvent(EventHandler Handler,object Sender,EventArgs Args)
        {
            if(Handler == null)
            {
                return;
            }
            Handler.Invoke(Sender, Args);
        }

        public static void DispatcherEvent<TEventArgs>(EventHandler<TEventArgs> Handler, object Sender, TEventArgs Args) where TEventArgs : EventArgs
        {
            if (Handler == null)
            {
                return;
            }
            Handler.Invoke(Sender, Args);
        }

        public static void AsyncDispatcherEvent(EventHandler Handler,object Sender,EventArgs Args,AsyncCallback EndInvokeCallback = null)
        {
            if (Handler == null)
            {
                return;
            }
            if(EndInvokeCallback == null)
            {
                EndInvokeCallback = DefaultEndCallback;
            }
            Handler.BeginInvoke(Sender, Args, EndInvokeCallback, Handler);
        }

        public static void AsyncDispatcherEvent<TEventArgs>(EventHandler<TEventArgs> Handler, object Sender, TEventArgs Args, AsyncCallback EndInvokeCallback = null) where TEventArgs : EventArgs
        {
            if (Handler == null)
            {
                return;
            }
            if (EndInvokeCallback == null)
            {
                EndInvokeCallback = DefaultEndCallbackArgs<TEventArgs>;
            }
            var deles = Handler.GetInvocationList();
            foreach (EventHandler<TEventArgs> handler in deles)
            {
                handler.BeginInvoke(Sender, Args, EndInvokeCallback, handler);
            }
            
        }

        public static void DispatcherEvent(Delegate Handler, object Sender, EventArgs Args)
        {
            if (Handler == null)
            {
                return;
            }
            var list = Handler.GetInvocationList();
            foreach (var item in list)
            {
                item.Method.Invoke(null, new object[] { Sender, Args });
            }
        }
    }
}
