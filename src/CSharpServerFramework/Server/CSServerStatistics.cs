using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpServerFramework.Server
{
    public class CSServerStatistics
    {
        private static CSServerStatistics _instance = new CSServerStatistics();
        public static CSServerStatistics Instance { get { return _instance; } }
        /// <summary>
        /// 吞吐率
        /// </summary>
        public int MaxThroughoutRate { get; set; }
    }
}
