using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.DataModel
{
    internal class DataSupplier: IDataSupplier
    {
        private readonly int _dataSupplierId;
        private readonly List<RegisteredSupply> _dataSupplies;

        public IList<Type> SuppliedTypes { get; private set; }
        public bool IsScoped { get; private set; }

        public DataSupplier(IIdManager idManager)
        {
            _dataSupplierId = idManager.GetUniqueId();
            _dataSupplies = new List<RegisteredSupply>();
            SuppliedTypes = new List<Type>();
        }

        void IDataSupplier.Add(IDataDependency dependency, Action<IRenderContext,IDataContext,IDataDependency> action)
        {
            if (ReferenceEquals(dependency.DataType, null))
                throw new Exception("Dependencies must specify a data type");

            _dataSupplies.Add(new RegisteredSupply 
                { 
                    DataSupplierId = _dataSupplierId,
                    Dependency = dependency,
                    Action = action
                });

            if (!SuppliedTypes.Contains(dependency.DataType))
                SuppliedTypes.Add(dependency.DataType);

            if (!string.IsNullOrEmpty(dependency.ScopeName))
                IsScoped = true;
        }

        bool IDataSupplier.IsSupplierOf(IDataDependency dependency)
        {
            return _dataSupplies.Any(s => s.IsMatch(dependency));
        }

        IDataDependency IDataSupplier.DefaultDependency
        {
            get
            {
                return _dataSupplies.Count == 0
                    ? null
                    : _dataSupplies[0].Dependency;
            }
        }

        IDataSupply IDataSupplier.GetSupply(IDataDependency dependency)
        {
            var supply = _dataSupplies.FirstOrDefault(s => s.IsMatch(dependency));

            return supply == null 
                ? null
                : new DataSupply 
                    {
                        DataSupplierId = _dataSupplierId,
                        Dependency = supply.Dependency,
                        Action = supply.Action,
                        IsStatic = true
                    };
        }

        public override string ToString()
        {
            var result = "supplier #" + _dataSupplierId;
            if (SuppliedTypes != null && SuppliedTypes.Count > 0)
            {
                result += " of [" + string.Join(", ", SuppliedTypes.Select(t => t.DisplayName(TypeExtensions.NamespaceOption.Ending))) + "]";
            }
            return result;
        }

        private class DataSupply : IDataSupply
        {
            public int DataSupplierId;
            public IDataDependency Dependency;
            public Action<IRenderContext, IDataContext, IDataDependency> Action;
            public bool IsStatic { get; set; }
            private readonly List<IDataSupply> _dependentSupplies = new List<IDataSupply>();

            public void Supply(IRenderContext renderContext, IDataContext dataContext)
            {
                Action(renderContext, dataContext, Dependency);

                int count;
                lock (_dependentSupplies) count = _dependentSupplies.Count;

                for (var i = 0; i < count; i++)
                {
                    IDataSupply dependent;
                    lock (_dependentSupplies) dependent = _dependentSupplies[i];
                    dependent.Supply(renderContext, dataContext);
                }
            }

            void IDataSupply.AddDependent(IDataSupply dataSupply)
            {
                lock (_dependentSupplies) _dependentSupplies.Add(dataSupply);
            }

            public override string ToString()
            {
                var data = Dependency == null ? "data" : Dependency.ToString();
                return (IsStatic ? "static " : "dynamic ") + data + " from supplier #" + DataSupplierId;
            }
        }

        private class RegisteredSupply
        {
            public int DataSupplierId;
            public IDataDependency Dependency;
            public Action<IRenderContext, IDataContext, IDataDependency> Action;

            public bool IsMatch(IDataDependency dependency)
            {
                if (dependency == null) return false;
                if (dependency.DataType != Dependency.DataType) return false;
                if (string.IsNullOrEmpty(dependency.ScopeName)) return true;
                if (string.IsNullOrEmpty(Dependency.ScopeName)) return true;
                return String.Equals(Dependency.ScopeName, dependency.ScopeName);
            }
        }
    }
}
