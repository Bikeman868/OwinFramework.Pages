﻿using System;
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

        protected override IRequestRouter GetImplementation(IMockProducer mockProducer)
        {
            return this;
        }

        public void Dispose()
        { }


        public IDisposable Register(IRunable runable, IRequestFilter filter, int priority, Type declaringType)
        {
            Runable = runable;
            Filter = filter;
            Priority = priority;
            DeclaringType = declaringType;
            return this;
        }

        public IDisposable Register(IRunable runable, IRequestFilter filter, int priority, MethodInfo methodInfo)
        {
            Runable = runable;
            Filter = filter;
            Priority = priority;
            MethodInfo = methodInfo;
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
