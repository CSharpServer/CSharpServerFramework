#define DEV_DEBUGN
using CSharpServerFramework.Server;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
namespace CSharpServerFramework.WorkThread
{
    /// <summary>
    /// 工作线程管理器
    /// 工作线程是调用相关的Extension处理接收请求的数据的线程
    /// 该管理器维护线程的数组，工作线程数至少有一个，最多根据配置增加
    /// 每一个工作线程有一个任务队列
    /// </summary>
    public class WorkThreadManager
    {
        protected CSServer Server { get { return CSServer.InternalInstance; } }
        protected Thread[] WorkerThreads { get; private set; }
        protected ConcurrentQueue<WorkerTask>[] WorkQueue { get; private set; }
        protected ManualResetEventSlim[] ThreadNewTaskAdded { get; private set; }
        protected int[] ThreadWaitTimes { get; private set; } //线程进入等待状态次数
        protected long[] ThreadDoWorkTimes { get; private set; }//线程完成的工作数
        /// <summary>
        /// 工作者线程数
        /// </summary>
        public int RunningWorkerThreadCount { get; protected set; }
        public WorkThreadManager()
        {
            ManagerInit();
        }

        public void Init()
        {
            if(Server.ServerState != CSServer.CSServerState.RUNNING)
            {
                return;
            }
            while (AddNewWorkerThread()); //增加工作线程到最大数量
            Server.Logger.Log("Work Thread Manager Inited");
        }

        protected bool AddNewWorkerThread()
        {
            if (RunningWorkerThreadCount == 0 || RunningWorkerThreadCount < CSServerBaseDefine.WORKER_THREAD_MAX)
            {
                //保证至少有一个工作线程
                int index = RunningWorkerThreadCount++;
                Thread newThread = new Thread(DoWorkProc);
                WorkerThreads[index] = newThread;
                try
                {
                    newThread.Start(index);
                    return true;
                }
                catch (Exception ex)
                {
                    Server.Logger.Log("Worker Thread Start Failed:" + ex.Message);
                    return false;
                }                
            }
            else
            {
                return false;
            }
        }

        private void DoWorkProc(object obj)
        {
            int index = int.Parse(obj.ToString());
            ConcurrentQueue<WorkerTask> workQueue = new ConcurrentQueue<WorkerTask>();
            WorkQueue[index] = workQueue;
            ManualResetEventSlim newTaskAdded = new ManualResetEventSlim(false);
            ThreadNewTaskAdded[index] = newTaskAdded;
            ThreadWaitTimes[index] = 0;
            ThreadDoWorkTimes[index] = 0;            
#if DEV_DEBUG
            Server.Logger.Log("Thread " + index + " Start");
#endif
            WorkerTask task;
            bool inWhileFlag = false;
            while (Server.ServerState == CSServer.CSServerState.RUNNING)
            {
                inWhileFlag = false;
                while (workQueue.Count > 0)
                {
                    if(workQueue.TryDequeue(out task))
                    {
                        try
                        {
                            task.DoWork();
                        }
                        catch (Exception ex)
                        {
                            Server.Logger.Log("Do Work Exception:" + ex.Message);
                        }
                    }
                    //完成工作数自增
                    ThreadDoWorkTimes[index]++;
                    inWhileFlag = true;
                }
                if(inWhileFlag)
                {
                    //进入等待时间次数自增
                    ThreadWaitTimes[index]++;
                }
                newTaskAdded.Reset();
                /**
                 * 每隔5秒检查一下队列(工作队列经常为空),增加的开销是每5秒一次的上下文切换
                 * 如果服务器消息频繁(工作队列不为空)，线程上下文正常切换，所以相对来说增加的开销等于0
                 */
                newTaskAdded.Wait(5 * 1000);
            }
#if DEV_DEBUG
            Server.Logger.Log("Thread " + index + " Stop");
#endif
        }

        protected void ManagerInit()
        {
            WorkerThreads = new Thread[CSServerBaseDefine.WORKER_THREAD_MAX];
            WorkQueue = new ConcurrentQueue<WorkerTask>[CSServerBaseDefine.WORKER_THREAD_MAX];
            ThreadNewTaskAdded = new ManualResetEventSlim[CSServerBaseDefine.WORKER_THREAD_MAX];
            ThreadDoWorkTimes = new long[CSServerBaseDefine.WORKER_THREAD_MAX];
            ThreadWaitTimes = new int[CSServerBaseDefine.WORKER_THREAD_MAX];
        }

        public void AddNewWork(WorkerTask Task)
        {
#if DEV_DEBUG
            Server.Logger.Log("AddNewWork Interval:" + (DateTime.UtcNow.Ticks - CSServerBaseDefine.ReceiveMessageTick));
#endif
            var workThreadId = Task.IsOrderTask ? Task.Session.WorkThreadId : GetUserWorkThread();
            WorkQueue[workThreadId].Enqueue(Task);
            ThreadNewTaskAdded[workThreadId].Set();
        }

        public int GetUserWorkThread()
        {
            int index = 0;
            for (int i = 1; i < CSServerBaseDefine.WORKER_THREAD_MAX; i++)
            {
                if (ThreadWaitTimes[i] > ThreadWaitTimes[index])
                {
                    //寻找最多进入等待状态的线程的索引                    
                    index = i;
                }
            }
            return index;
        }

        public void Stop()
        {
            for (int i = 0; i < ThreadNewTaskAdded.Length; i++)
            {
                if(ThreadNewTaskAdded[i]!=null)
                {
                    ThreadNewTaskAdded[i].Set();
                }
            }
        }
    }
}
