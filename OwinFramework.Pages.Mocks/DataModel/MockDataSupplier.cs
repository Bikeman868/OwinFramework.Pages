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

        public class DataSupplier : IDataSupplier, IDataSupply
        {
            public IList<Type> SuppliedTypes { get; private set; }
            public bool IsScoped { get; private set; }
            public IDataDependency DefaultDependency { get { return _dependency; } }
            public bool IsStatic { get { return true; } set { } }
            private readonly List<Action<IRenderContext>> _dependentSupplies = new List<Action<IRenderContext>>();

            private IDataDependency _dependency;
            private Action<IRenderContext, IDataContext, IDataDependency> _action;

            public void Add(
                IDataDependency dependency, 
                Action<IRenderContext, IDataContext, IDataDependency> action)
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

            void IDataSupply.AddOnSupplyAction(Action<IRenderContext> renderAction)
            {
                _dependentSupplies.Add(renderAction);
            }

            public void Supply(IRenderContext renderContext, IDataContext dataContext)
            {
                _action(renderContext, dataContext, _dependency);
                foreach (var dependent in _dependentSupplies)
                    dependent(renderContext);
            }
        }
    }
}
