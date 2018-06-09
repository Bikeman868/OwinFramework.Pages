using System;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Facilities.Runtime
{
    internal class DataContext: IDataContext, IDisposable
    {
        public IOwinContext OwinContext { get; private set; }

        public IDataContext Initialize(IOwinContext context)
        {
            OwinContext = context;
            return this;
        }

        public void Dispose()
        {
        }
    }
}
