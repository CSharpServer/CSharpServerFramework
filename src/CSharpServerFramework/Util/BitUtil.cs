using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpServerFramework.Util
{
    public class BitUtil
    {
        /// <summary>
        /// 封装包头到字节流
        /// </summary>
        /// <param name="DataWithHead">封装包头的容器</param>
        /// <param name="Data">数据流</param>
        /// <param name="Length">数据流长度</param>
        /// <returns>封装后总长度</returns>
        public static int CreateDataPackageWithHead(byte[] DataWithHead, byte[] Data, int Length)
        {
            byte[] dataHead = BitConverter.GetBytes(Length);
            Array.Copy(dataHead, DataWithHead, dataHead.Length);
            Array.Copy(Data, 0, DataWithHead, dataHead.Length, Length);
            return dataHead.Length + Length;
        }
    }
}
