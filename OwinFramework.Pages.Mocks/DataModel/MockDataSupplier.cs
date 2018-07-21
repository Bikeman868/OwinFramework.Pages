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
            public IList<Type> SuppliedTypes { get; set; }
            public bool IsScoped { get; set; }
            public IDataDependency Dependency;
            public Action<IRenderContext, IDataContext, IDataDependency> Action;

            public void Add(IDataDependency dependency, Action<IRenderContext, IDataContext, IDataDependency> action)
            {
                Dependency = dependency;
                Action = action;
                SuppliedTypes = new List<Type> { dependency.DataType };
                IsScoped = !string.IsNullOrEmpty(dependency.ScopeName);
            }

            public bool IsSupplierOf(IDataDependency dependency)
            {
                return Dependency == null || Dependency.DataType == dependency.DataType;
            }

            public IDataSupply GetSupply(IDataDependency dependency)
            {
                return this;
            }

            public void Supply(IRenderContext renderContext, IDataContext dataContext)
            {
                Action(renderContext, dataContext, Dependency);

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

            public IList<IDataSupply> DependentSupplies
            {
                get { return null; }
            }

            public event EventHandler<DataSuppliedEventArgs> OnDataSupplied;
        }
    }
}
