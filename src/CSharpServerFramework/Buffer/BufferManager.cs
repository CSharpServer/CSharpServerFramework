#define DEV_DEBUGn
using CSharpServerFramework.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpServerFramework.Buffer
{
    /// <summary>
    /// 缓存管理器
    /// 负责分配和释放缓存
    /// </summary>
    public class BufferManager
    {
        protected CSServer Server { get { return CSServer.InternalInstance; } }
        protected IList<CSServerBuffer> BufferList { get; private set; }
        private object _getFreeBufferLock = new object();
        private volatile int _nextFreeBufferIndex;
        public BufferManager()
        {
            ManagerInit();
        }

        private void ManagerInit()
        {
            BufferList = new List<CSServerBuffer>();
        }

        public void Init()
        {            
            AddNewBufferToList(CSServerBaseDefine.DEFAULT_BUFFER_LIST_SIZE);
            Server.Logger.Log("Buffer Manager Inited");
        }

        /// <summary>
        /// 扩充缓存区
        /// </summary>
        /// <param name="AddSize">扩充Buffer数量</param>
        /// <returns>扩充的第一个Buffer的在BufferList的索引</returns>
        protected int AddNewBufferToList(uint AddSize)
        {
            if (BufferList.Count + AddSize > CSServerBaseDefine.MAX_BUFFER_SIZE)
            {
                AddSize = CSServerBaseDefine.MAX_BUFFER_SIZE - (uint)BufferList.Count;
                if (AddSize == 0)
                {
                    return 0;
                }
            }
            int idSeed = BufferList.Count;
            for (int i = 0; i < AddSize; i++)
            {
                CSServerBuffer newBuff = new CSServerBuffer(new byte[CSServerBaseDefine.BUFFER_SIZE], idSeed + i) { IsUsing = false };
                BufferList.Add(newBuff);
            }
#if DEV_DEBUG
                Console.WriteLine("AddNewBufferToList");
#endif

            return idSeed;
        }
        public void FreeBuffer(int Id)
        {
            if ( BufferList.Count > Id)
            {
                BufferList[Id].Free();
            }
        }
        /// <summary>
        /// 获取空闲缓存
        /// </summary>
        /// <returns>缓存实例</returns>
        public CSServerBuffer GetFreeBuffer()
        {
            //return new CSServerBuffer(new byte[1024 * 32],0);
            int index = (_nextFreeBufferIndex) % BufferList.Count; ;
            bool notfindflag = true;
            lock (_getFreeBufferLock)
            {

                for (int i = 0; i < BufferList.Count; i++)
                {
                    index = (_nextFreeBufferIndex + i) % BufferList.Count;
                    if (!BufferList[index].IsUsing)
                    {
                        notfindflag = false;
                        break;
                    }
                }
                if (notfindflag)
                {
                    index = AddNewBufferToList(Server.ServerConfig.GetBufferAddPerTimeCount());
                }
            }
            var freeBuffer = BufferList[index];
            freeBuffer.IsUsing = true;
            freeBuffer.NextBuffer = null;
            freeBuffer.TailBuffer = null;
            _nextFreeBufferIndex = (index + 1) % BufferList.Count;
            return freeBuffer;
        }
        public void Dispose()
        {
            for (int i = 0; i < BufferList.Count; i++)
            {
                BufferList[i].Dispose();
                BufferList[i] = null;
            }
        }
    }
}
