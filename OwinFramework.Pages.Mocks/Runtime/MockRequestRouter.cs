using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Owin;
using Moq.Modules;
using OwinFramework.InterfacesV1.Capability;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Mocks.Runtime
{
    public class MockRequestRouter: ConcreteImplementationProvider<IRequestRouter>, IRequestRouter, IDisposable
    {
        public IRequestFilter Filter;
        public IRunable Runable;
        public int Priority;
        public Type DeclaringType;
        public MethodInfo MethodInfo;
        public string UserSegmentKey;

        protected override IRequestRouter GetImplementation(IMockProducer mockProducer)
        {
            return this;
        }

        public void Dispose()
        { }


        public IDisposable Register(IRunable runable, IRequestFilter filter, int priority, Type declaringType, string userSegmentKey)
        {
            Runable = runable;
            Filter = filter;
            Priority = priority;
            DeclaringType = declaringType;
            UserSegmentKey = userSegmentKey;
            return this;
        }

        public IDisposable Register(IRunable runable, IRequestFilter filter, int priority, MethodInfo methodInfo, string userSegmentKey)
        {
            Runable = runable;
            Filter = filter;
            Priority = priority;
            MethodInfo = methodInfo;
            UserSegmentKey = userSegmentKey;
            return this;
        }

        public IDisposable Register(IRequestRouter router, IRequestFilter filter, int priority, string userSegmentKey)
        {
            return this;
        }

        public IList<IEndpointDocumentation> GetEndpointDocumentation()
        {
            return null;
        }

        public IRequestRouter Add(IRequestFilter filter, int priority, string userSegmentKey)
        {
            return this;
        }

        public IRunable Route(
            IOwinContext context, 
            Action<IOwinContext, Func<string>> trace,
            string absolutePath,
            Method rewriteMethod)
        {
            return Runable;
        }
    }
}
