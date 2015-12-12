using CSharpServerFramework.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpServerFramework.Message
{
    public class MessageAsyncState : CSServerAsyncState
    {
        public int LeftDataLength { get; set; }
        public MessageAsyncState(object State) :base(State)
        {

        }
    }
}
