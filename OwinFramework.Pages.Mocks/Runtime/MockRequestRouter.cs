using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Owin;
using Moq.Modules;
using OwinFramework.InterfacesV1.Capability;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Mocks.Runtime
{
    public class MockRequestRouter: ConcreteImplementationProvider<IRequestRouter>, IRequestRouter, IDisposable
    {
        public IRequestFilter Filter;
        public IRunable Runable;
        public int Priority;
        public Type DeclaringType;

        protected override IRequestRouter GetImplementation(IMockProducer mockProducer)
        {
            return this;
        }

        public void Dispose()
        { }

        public IRunable Route(IOwinContext context)
        {
            return Runable;
        }

        public IDisposable Register(IRunable runable, IRequestFilter filter, int priority, Type declaringType)
        {
            Runable = runable;
            Filter = filter;
            Priority = priority;
            DeclaringType = declaringType;
            return this;
        }

        public IDisposable Register(IRequestRouter router, IRequestFilter filter, int priority = 0)
        {
            return this;
        }

        public IList<IEndpointDocumentation> GetEndpointDocumentation()
        {
            return null;
        }

        public IRequestRouter Add(IRequestFilter filter, int priority)
        {
            return this;
        }
    }
}
