using CSharpServerFramework.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CSharpServerFramework.Log
{
    /// <summary>
    /// 记录器
    /// 启动一条线程进行Log记录
    /// </summary>
    public class CSServerLogger
    {
        private Thread _logThread;
        private volatile bool _running = false;
        private System.Collections.Concurrent.ConcurrentQueue<string> _logQueue;
        private ManualResetEventSlim _nextLogAdded;
        private IList<ILoggerLog> _loggers;
        public CSServerLogger()
        {
            _loggers = new List<ILoggerLog>();
            _nextLogAdded = new ManualResetEventSlim(false);
        }

        public void AddLogger(ILoggerLog Logger)
        {
            _loggers.Add(Logger);
        }

        public void Init()
        {
            _logThread = new Thread(DoLogProc);
            _logQueue = new System.Collections.Concurrent.ConcurrentQueue<string>();
            _running = true;
            _logThread.Start();
            Log("Logger Inited");
        }

        private void DoLogProc(object obj)
        {
            while (_running)
            {
                string log;
                while (_logQueue.Count > 0)
                {
                    try
                    {
                        var flag = _logQueue.TryDequeue(out log);
                        if (flag)
                        {
                            foreach (var logger in _loggers)
                            {
                                logger.Log(log);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log("CSServer Logger:" + ex.Message);
                    }
                }
                try
                {
                    _nextLogAdded.Reset();
                    _nextLogAdded.Wait(5 * 1000);
                }
                catch (Exception ex)
                {
                    Log("CSServer Logger:" + ex.Message);
                }
                
            }
        }
        public void Log(string LogText)
        {
            try
            {
                string log = string.Format("<{0}>:{1}", DateTime.Now.ToString(), LogText);
                _logQueue.Enqueue(log);
                _nextLogAdded.Set();
            }
            catch (Exception)
            {
                _nextLogAdded.Set();
            }
            
        }
        public void Stop()
        {
            _running = false;
            foreach (var logger in _loggers)
            {
                logger.Close();
            }
            _nextLogAdded.Set();
        }
    }
}
