using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ninject;
using Prius.Contracts.Interfaces.External;

namespace Sample5.PriusIntegration
{
    internal class PriusFactory: IFactory
    {
        public static StandardKernel Ninject;

        object IFactory.Create(Type type)
        {
            return Ninject.Get(type);
        }

        T IFactory.Create<T>()
        {
            return Ninject.Get<T>();
        }
    }
}