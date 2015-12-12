#define DEV_DEBUGN
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpServerFramework.Server
{
    /// <summary>
    /// 服务器的基本定义
    /// </summary>
    internal class CSServerBaseDefine
    {
        /// <summary>
        /// Buffer的长度，单位byte
        /// </summary>
        public static int BUFFER_SIZE = 4 * 1024;
        /// <summary>
        /// 工作线程数量
        /// </summary>
        public static int WORKER_THREAD_MAX = 4;
        /// <summary>
        /// 包头长度
        /// </summary>
        public const int TCP_PACKAGE_HEAD_SIZE = 4; //Int32 类型 ,4位
        /// <summary>
        /// 网络通信超时时间，毫秒
        /// </summary>
        public static int TIME_OUT = 15000;
        /// <summary>
        /// 初始化缓存数量
        /// </summary>
        public static uint DEFAULT_BUFFER_LIST_SIZE = 200;

        /// <summary>
        /// 最大缓存数
        /// </summary>
        public static uint MAX_BUFFER_SIZE = 4096;

#if DEV_DEBUG
        public static long ReceiveMessageTick;
#endif

    }
}
