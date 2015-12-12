using CSharpServerFramework.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpServerFramework.Server
{
    /// <summary>
    /// 服务器异步调用的附带数据
    /// </summary>
    public class CSServerAsyncState
    {
        public object State { get; private set; }
        public CSServerAsyncState(object StateObject)
        {
            State = StateObject;
        }
    }
}
