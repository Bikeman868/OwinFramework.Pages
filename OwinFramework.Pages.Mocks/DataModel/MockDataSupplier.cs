using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq.Modules;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Mocks.DataModel
{
    public class MockDataSupplier: ConcreteImplementationProvider<IDataSupplier>
    {
        protected override IDataSupplier GetImplementation(IMockProducer mockProducer)
        {
            return new DataSupplier();
        }

        private class DataSupplier : IDataSupplier, IDataSupply
        {
            public IList<Type> SuppliedTypes { get; private set; }
            public bool IsScoped { get; private set; }
            public IDataDependency DefaultDependency { get { return _dependency; } }
            public bool IsStatic { get { return true; } }
            public event EventHandler<DataSuppliedEventArgs> OnDataSupplied;
            public IList<IDataSupply> DependentSupplies { get { return null; } }

            private IDataDependency _dependency;
            private Action<IRenderContext, IDataContext, IDataDependency> _action;

            public void Add(
                IDataDependency dependency, 
                Action<IRenderContext, IDataContext, IDataDependency> action,
                bool isStatic)
            {
                _dependency = dependency;
                _action = action;
                SuppliedTypes = new List<Type> { dependency.DataType };
                IsScoped = !string.IsNullOrEmpty(dependency.ScopeName);
            }

            public bool IsSupplierOf(IDataDependency dependency)
            {
                return _dependency == null || _dependency.DataType == dependency.DataType;
            }

            public IDataSupply GetSupply(IDataDependency dependency)
            {
                return this;
            }

            public void Supply(IRenderContext renderContext, IDataContext dataContext)
            {
                _action(renderContext, dataContext, _dependency);

                if (OnDataSupplied != null)
                {
                    var args = new DataSuppliedEventArgs
                    {
                        RenderContext = renderContext,
                        DataContext = dataContext
                    };
                    OnDataSupplied(this, args);
                }
            }
        }
    }
}
