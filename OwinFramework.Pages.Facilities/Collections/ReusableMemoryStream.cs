using System;
using System.IO;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Facilities.Collections
{
    /// <summary>
    /// An implementation of IMemoryStream that is re-usable
    /// </summary>
    internal class ReusableMemoryStream : ReusableObject, IMemoryStream
    {
        private readonly MemoryStream _memoryStream;

        public ReusableMemoryStream()
        {
            _memoryStream = new MemoryStream(8000);
        }

        public new IMemoryStream Initialize(Action<IReusable> disposeAction)
        {
            base.Initialize(disposeAction);
            _memoryStream.Position = 0;
            _memoryStream.SetLength(0);
            return this;
        }

        public Stream AsStream()
        {
            return _memoryStream;
        }

        public byte[] ToArray()
        {
            return _memoryStream.ToArray();
        }

        public void Write(byte[] buffer, long offset, long count)
        {
            _memoryStream.Write(buffer, (int)offset, (int)count);
        }

        public void WriteByte(byte value)
        {
            _memoryStream.WriteByte(value);
        }

        public long Length
        {
            get { return _memoryStream.Length; }
        }

        public long Position
        {
            get { return _memoryStream.Position; }
            set { _memoryStream.Position = value; }
        }

        public void CopyTo(Stream stream)
        {
            _memoryStream.CopyTo(stream);
        }
    }
}
