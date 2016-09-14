using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Threading.Tasks.Dataflow;
using System.Threading.Tasks;

namespace CSharpServerFramework.Util
{
    public class CommandListener
    {
        private UdpClient listenerClient;
        private int _port = 8034;
        private volatile bool _running;
        private Thread _commandListenerThread;
        /// <summary>
        /// 新命令传递进来触发
        /// </summary>
        public event EventHandler<CommandArgs> OnCommandCall;
        public CommandListener()
        {
            Init();
        }

        private void Init()
        {

        }

        /// <summary>
        /// 开启命令监听
        /// </summary>
        /// <param name="Port"></param>
        public void StartListener(int Port)
        {
            if (_running)
            {
                throw new Exception("Command Listener Is Running");
            }
            else
            {
                _running = true;
                _port = Port;
                _commandListenerThread = new Thread(DoListenOrder);
                _commandListenerThread.Start();
            }
        }

        public void StopListener()
        {
            if (_running)
            {
                _running = false;
                try
                {
                    listenerClient.Dispose();
                }
                catch (Exception)
                {
                }
            }
        }

        private void DoListenOrder()
        {
            UdpClient udpClient = new UdpClient(this._port);
            listenerClient = udpClient;
            Task.Run(async () =>
            {
                while (_running)
                {
                    try
                    {

                        var a = await udpClient.ReceiveAsync();
                        string command = UTF8Encoding.UTF8.GetString(a.Buffer, 0, a.Buffer.Length);
                        DoCommand(command);
                        if (!_running)
                        {
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            });
        }

        private void DoCommand(string command)
        {
            string[] args;
            if (string.IsNullOrWhiteSpace(command))
            {
                args = new string[0];
            }
            else
            {
                var cmds = command.Split(new char[] { ' ' });
                IList<string> list = new List<string>();
                foreach (var item in cmds)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        list.Add(item);
                    }
                }
                args = list.ToArray();
            }
            var eventArgs = new CommandArgs() { Args = args };
            try
            {
                EventDispatcherUtil.AsyncDispatcherEvent<CommandArgs>(OnCommandCall, this, eventArgs);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public class CommandArgs : EventArgs
    {
        /// <summary>
        /// 外部传递的参数数组
        /// </summary>
        public string[] Args { get; set; }
    }
}