using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpServerFramework.Client
{
    public class ClientManagerEventArgs:EventArgs
    {
        public object State { get; set; }
        public UserSession Session { get; set; }
    }
}
