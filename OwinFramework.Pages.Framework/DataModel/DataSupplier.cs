using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.DataModel
{
    internal class DataSupplier: IDataSupplier, IDebuggable
    {
        private readonly int _dataSupplierId;
        private readonly List<RegisteredSupply> _dataSupplies;

        public IList<Type> SuppliedTypes { get; private set; }

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
                    Dependency = dependency,
                    Action = action
                });

            if (!SuppliedTypes.Contains(dependency.DataType))
                SuppliedTypes.Add(dependency.DataType);
        }

        bool IDataSupplier.CanSupplyScoped(Type type)
        {
            return _dataSupplies.Any(s => 
                s.Dependency.DataType == type && 
                !string.IsNullOrEmpty(s.Dependency.ScopeName));
        }

        bool IDataSupplier.CanSupplyUnscoped(Type type)
        {
            return _dataSupplies.Any(s =>
                s.Dependency.DataType == type &&
                string.IsNullOrEmpty(s.Dependency.ScopeName));
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
                        DataSupplier = this,
                        Dependency = supply.Dependency,
                        Action = supply.Action,
                        IsStatic = true
                    };
        }

        T IDebuggable.GetDebugInfo<T>(int parentDepth, int childDepth)
        {
            return new DebugDataSupplier
            {
                Instance = this,
                Name = "supplier #" + _dataSupplierId,
                SuppliedTypes = SuppliedTypes == null ? null : SuppliedTypes.ToList(),
            } as T;
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

        private class DataSupply : IDataSupply, IDebuggable
        {
            public DataSupplier DataSupplier;
            public IDataDependency Dependency;
            public Action<IRenderContext, IDataContext, IDataDependency> Action;
            public bool IsStatic { get; set; }

            private readonly List<Action<IRenderContext>> _onSupplyActions = new List<Action<IRenderContext>>();

            public void Supply(IRenderContext renderContext, IDataContext dataContext)
            {
                Action(renderContext, dataContext, Dependency);

                int count;
                lock (_onSupplyActions) count = _onSupplyActions.Count;

                for (var i = 0; i < count; i++)
                {
                    Action<IRenderContext> action;
                    lock (_onSupplyActions) action = _onSupplyActions[i];
                    action(renderContext);
                }
            }

            void IDataSupply.AddOnSupplyAction(Action<IRenderContext> onSupplyAction)
            {
                lock (_onSupplyActions) _onSupplyActions.Add(onSupplyAction);
            }

            public override string ToString()
            {
                var data = Dependency == null ? "data" : Dependency.ToString();
                return (IsStatic ? "static " : "dynamic ") + data + " from supplier #" + DataSupplier._dataSupplierId;
            }

            T IDebuggable.GetDebugInfo<T>(int parentDepth, int childDepth)
            {
                var result = new DebugDataSupply
                {
                    Instance = this,
                    IsStatic = IsStatic,
                    SubscriberCount = _onSupplyActions.Count,
                    SuppliedData = new DebugDataScope
                    {
                        DataType = Dependency.DataType,
                        ScopeName = Dependency.ScopeName
                    },
                };

                if (parentDepth != 0)
                    result.Supplier = DataSupplier.GetDebugInfo<DebugDataSupplier>(parentDepth - 1, 0);

                return result as T;
            }
        }

        private class RegisteredSupply
        {
            public IDataDependency Dependency;
            public Action<IRenderContext, IDataContext, IDataDependency> Action;

            public bool IsMatch(IDataDependency dependency)
            {
                if (dependency == null) return false;
                if (dependency.DataType != Dependency.DataType) return false;
                if (string.IsNullOrEmpty(dependency.ScopeName)) return true;
                if (string.IsNullOrEmpty(Dependency.ScopeName) && string.IsNullOrEmpty(dependency.ScopeName)) return true;
                return String.Equals(Dependency.ScopeName, dependency.ScopeName, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
