using System;
using OwinFramework.Pages.Core.Interfaces.Collections;

namespace OwinFramework.Pages.Facilities.Collections
{
    /// <summary>
    /// Implementation of IMemoryStreamFactory that pools and reuses memory streams
    /// </summary>
    internal class MemoryStreamFactory : ReusableObjectFactory, IMemoryStreamFactory
    {
        public MemoryStreamFactory(IQueueFactory queueFactory)
            : base(queueFactory)
        {
            Initialize(100);
        }

        public IMemoryStream Create()
        {
            var memoryStream = (ReusableMemoryStream)Queue.DequeueOrDefault();
            if (memoryStream == null)
                memoryStream = new ReusableMemoryStream();
            return memoryStream.Initialize(DisposeAction);
        }

        public IMemoryStream Create(Byte[] data)
        {
            var result = Create();
            result.Write(data, 0, data.LongLength);
            result.Position = 0;
            return result;
        }
    }
}
