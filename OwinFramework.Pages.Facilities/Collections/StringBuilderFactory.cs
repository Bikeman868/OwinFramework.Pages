using System;
using OwinFramework.Pages.Core.Interfaces.Collections;

namespace OwinFramework.Pages.Facilities.Collections
{
    /// <summary>
    /// Implementation of IStringBuilderFactory that pools and reuses string builders
    /// </summary>
    internal class StringBuilderFactory : ReusableObjectFactory, IStringBuilderFactory
    {
        private readonly IArrayFactory _arrayFactory;
        private const Int64 _defaultCapacity = 8000;

        public StringBuilderFactory(IQueueFactory queueFactory, IArrayFactory arrayFactory)
            : base(queueFactory)
        {
            _arrayFactory = arrayFactory;
            base.Initialize(100);
        }

        public IStringBuilder Create(long capacity)
        {
            var stringBuilder = (ReusableStringBuilder)Queue.DequeueOrDefault();
            if (stringBuilder == null)
                stringBuilder = new ReusableStringBuilder(_arrayFactory);
            return stringBuilder.Initialize(DisposeAction, capacity);
        }

        public IStringBuilder Create()
        {
            return Create(_defaultCapacity);
        }

        public IStringBuilder Create(string data)
        {
            var result = Create(0);
            result.Append(data);
            return result;
        }
    }
}
