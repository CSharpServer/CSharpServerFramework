using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpServerFramework.Buffer
{
    /// <summary>
    /// 缓存
    /// </summary>
    public class CSServerBuffer
    {
        /// <summary>
        /// 获取Buffer的Id
        /// </summary>
        internal int Id { get; private set; }
        /// <summary>
        /// Buffer节点的数据
        /// </summary>
        public byte[] Buffer { get; private set; }
        /// <summary>
        /// 获取或设置单个Buffer节点的实际数据长度
        /// </summary>
        public int BufferLength { get; set; }
        
        private volatile bool _isUsing;
        private byte[] _totalBuffer = null;

        /// <summary>
        /// Buffer是否在使用
        /// </summary>
        internal bool IsUsing
        {
            get { return _isUsing; }
            set { _isUsing = value; }
        }

        internal CSServerBuffer NextBuffer { get; set; }
        internal CSServerBuffer TailBuffer { get; set; }

        public int BufferTotalLength
        {
            get
            {
                var len = AllBuffer.Length;
                if(_totalBuffer == null)
                {
                    return BufferLength;
                }
                else
                {
                    return len;
                }
            }
        }

        public byte[] AllBuffer
        {
            get
            {
                if(_totalBuffer == null)
                {
                    if(NextBuffer == null)
                    {
                        return Buffer;
                    }
                    else
                    {
                        _totalBuffer = new byte[BufferLength + NextBuffer.BufferTotalLength];
                        Array.Copy(Buffer, 0, _totalBuffer, 0, BufferLength);
                        Array.Copy(NextBuffer.AllBuffer, 0, _totalBuffer, BufferLength, NextBuffer.BufferTotalLength);
                    }
                }
                return _totalBuffer;
            }
        }

        public CSServerBuffer(byte[] Buffer,int Id)
        {
            this.Buffer = Buffer;
            this.Id = Id;
        }

        public void Free()
        {
            _isUsing = false;
            if(NextBuffer!=null)
            {
                NextBuffer.Free();
            }
            NextBuffer = null;
        }

        public void Dispose()
        {
            Buffer = null;
        }
    }
}
