using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Framework.Utility;

namespace OwinFramework.Pages.Framework.DataModel
{
    public class DataContextBuilder : IDataContextBuilder
    {
        public int Id { get; private set; }

        private readonly IDataContextFactory _dataContextFactory;
        private readonly IIdManager _idManager;
        private readonly IDataCatalog _dataCatalog;

        private DataContextBuilder[] _children;
        private DataContextBuilder _parent;

        private readonly List<IDataScope> _dataScopes;
        private readonly List<IDataSupply> _requiredDataSupplies;
        private readonly List<SuppliedDependency> _requiredSuppliedDependencies;
        private readonly List<IDataConsumer> _dataConsumers = new List<IDataConsumer>();

        private List<IDataSupply> _dataSupplies;
        private List<SuppliedDependency> _suppliedDependencies;

        private IDataSupply[] _staticSupplies;

        public DataContextBuilder(
            IDataContextFactory dataContextFactory,
            IIdManager idManager,
            IDataCatalog dataCatalog,
            IDataScopeRules dataScopeRules)
        {
            _dataContextFactory = dataContextFactory;
            _idManager = idManager;
            _dataCatalog = dataCatalog;

            Id = idManager.GetUniqueId();

            _dataScopes = dataScopeRules.DataScopes == null 
                ? new List<IDataScope>() 
                : dataScopeRules.DataScopes.ToList();

            _requiredDataSupplies = dataScopeRules.DataSupplies == null
                ? new List<IDataSupply>()
                : dataScopeRules.DataSupplies.ToList();

            _requiredSuppliedDependencies = dataScopeRules.SuppliedDependencies == null
                ? new List<SuppliedDependency>()
                : dataScopeRules.SuppliedDependencies.Select(sd => new SuppliedDependency(sd)).ToList();
        }

        public IDataContextBuilder AddChild(IDataScopeRules dataScopeRules)
        {
            var child = new DataContextBuilder(
                _dataContextFactory, 
                _idManager, 
                _dataCatalog, 
                dataScopeRules)
            {
                _parent = this
            };

            if (_children == null)
                _children = new[] { child };
            else
                _children = _children.Concat(new[] { child }).ToArray();

            return child;
        }

        public void AddConsumer(IDataConsumer consumer)
        {
            _dataConsumers.Add(consumer);
        }

        public void ResolveSupplies()
        {
#if TRACE
            Trace.WriteLine("Data context builder #" + Id + " resolving suppliers");
#endif
            _dataSupplies = new List<IDataSupply>();
            _suppliedDependencies = new List<SuppliedDependency>();

            foreach (var supply in _requiredDataSupplies) AddSupply(supply);
            foreach (var supplier in _requiredSuppliedDependencies) AddDataSupplier(supplier);
            foreach (var consumer in _dataConsumers) Resolve(consumer);

            if (_children != null)
            {
                for (var i = 0; i < _children.Length; i++)
                    _children[i].ResolveSupplies();

            }

            var sortedList = GetSuppliedDependenciesOrdered();

            foreach (var sd in sortedList)
                AddSupply(sd.DataSupply);

            _staticSupplies = _dataSupplies.Where(s => s.IsStatic).ToArray();
        }

        public void SetupDataContext(IRenderContext renderContext)
        {
            var rootContext = _dataContextFactory.Create(renderContext, this);
            AddDataContext(renderContext, rootContext);
            renderContext.SelectDataContext(Id);
        }

        public bool IsInScope(IDataDependency dependency)
        {
            if (_parent == null) return true;
            if (_dataScopes == null) return false;
            return _dataScopes.Any(scope => scope.IsMatch(dependency));
        }

        public void AddMissingData(IRenderContext renderContext, IDataDependency missingDependency)
        {
#if TRACE
            renderContext.Trace(() => "Data context builder #" + Id + " has been notified of a missing dependency on " + missingDependency);
#endif

            Resolve(missingDependency);

            var root = this;
            while (!ReferenceEquals(root._parent, null))
                root = root._parent;

            renderContext.DeleteDataContextTree();
            root.SetupDataContext(renderContext);
        }

        #region private implementation details

        /// <summary>
        /// Adds a data supply to this context without adding the same supply twice
        /// </summary>
        private void AddSupply(IDataSupply supply)
        {
            if (_dataSupplies.All(s => s != supply))
            {
#if TRACE
                Trace.WriteLine("Data context builder #" + Id + " adding supply '" + "'");
#endif
                _dataSupplies.Add(supply);
                Resolve(supply as IDataConsumer);
            }
        }

        /// <summary>
        /// Adds a data supplier and specific type of data to supply to this
        /// context without duplicating data in the same scope
        /// </summary>
        private void AddDataSupplier(SuppliedDependency suppliedDependency)
        {
            if (_suppliedDependencies.All(s => !Equals(s.DataDependency, suppliedDependency.DataDependency)))
            {
                _suppliedDependencies.Add(suppliedDependency);
                Resolve(suppliedDependency.DataSupplier as IDataConsumer);
            }
        }

        /// <summary>
        /// Resolves all of the data needs of a data consumer
        /// </summary>
        private void Resolve(IDataConsumer consumer)
        {
            if (consumer == null) return;
            var needs = consumer.GetConsumerNeeds();

            if (needs.DataSupplyDependencies != null)
                foreach (var supply in needs.DataSupplyDependencies) 
                    AddSupply(supply);

            if (needs.DataDependencies != null)
            {
                foreach (var dependency in needs.DataDependencies)
                    Resolve(dependency);
            }

            if (needs.DataSupplierDependencies != null)
            {
                foreach (var dataSupplier in needs.DataSupplierDependencies)
                {
                    Resolve(new SuppliedDependency(dataSupplier), true);
                }
            }
        }

        private void Resolve(IDataDependency dependency)
        {
            if (IsSupplierOf(dependency)) return;

            if (IsInScope(dependency))
            {
                var supplier = _dataCatalog.FindSupplier(dependency);
                if (supplier == null)
                    throw new Exception("The data catalog does not contain a supplier of '" + dependency + "'");

                AddDataSupplier(new SuppliedDependency(supplier, dependency));
            }
            else
            {
                _parent.Resolve(dependency);
            }
        }

        private bool Resolve(SuppliedDependency suppliedDependency, bool addIfMissing)
        {
            if (IsSupplierOf(suppliedDependency.DataDependency)) return true;

            if (_parent != null && _parent.Resolve(suppliedDependency, false))
                return true;

            if (addIfMissing)
            {
                if (IsInScope(suppliedDependency.DataDependency))
                {
                    AddDataSupplier(suppliedDependency);

                    var supplier = suppliedDependency.DataSupplier;
                    Resolve(supplier as IDataConsumer);

                    return true;
                }
                return _parent.Resolve(suppliedDependency, true);
            }
            return false;
        }

        /// <summary>
        /// Returns true if this instance is already supplying this type of data
        /// </summary>
        private bool IsSupplierOf(IDataDependency dependency)
        {
            if (_suppliedDependencies == null) return false;
            return _suppliedDependencies.Any(sd => Equals(sd.DataDependency, dependency));
        }

        /// <summary>
        /// Returns a list of the supplied dependencies sorted such that all list entries
        /// appear after all of the entries that they depend on
        /// </summary>
        private List<SuppliedDependency> GetSuppliedDependenciesOrdered()
        {
            Func<SuppliedDependency, SuppliedDependency, bool> isDependentOn = (d1, d2) =>
            {
                if (d1.DependentSupplies == null || d1.DependentSupplies.Count == 0) return false;
                if (ReferenceEquals(d2.DataSupply, null)) return false;
                return d1.DependentSupplies.Any(s => ReferenceEquals(s, d2.DataSupply));
            };
            var listSorter = new DependencyListSorter<SuppliedDependency>();

            lock (_suppliedDependencies)
                return listSorter.Sort(_suppliedDependencies, isDependentOn);
        }

        /// <summary>
        /// Recursively builds a data context tree for a render context
        /// </summary>
        private void BuildDataContextTree(IRenderContext renderContext, IDataContext parentDataContext)
        {
            var dataContext = parentDataContext.CreateChild(this);
            AddDataContext(renderContext, dataContext);
        }

        /// <summary>
        /// Recursively builds a data context tree for a render context
        /// </summary>
        private void AddDataContext(IRenderContext renderContext, IDataContext dataContext)
        {
            if (_staticSupplies != null)
            {
                for (var i = 0; i < _staticSupplies.Length; i++)
                    _staticSupplies[i].Supply(renderContext, dataContext);
            }

            renderContext.AddDataContext(Id, dataContext);

            if (_children != null)
            {
                foreach (var child in _children)
                    child.BuildDataContextTree(renderContext, dataContext);
            }
        }

        #endregion

        #region DataSupplier class

        private class SuppliedDependency
        {
            public IDataSupplier DataSupplier;
            public IDataDependency DataDependency;
            public IDataSupply DataSupply;
            public List<IDataSupply> DependentSupplies;

            public SuppliedDependency(IDataSupplier dataSupplier, IDataDependency dataDependency)
            {
                DataSupplier = dataSupplier;
                DataDependency = dataDependency;
                DependentSupplies = new List<IDataSupply>();

                if (DataDependency != null && !DataSupplier.IsSupplierOf(DataDependency))
                    throw new Exception("Supplier '" + DataSupplier + "' is not a supplier of '" + DataDependency + "'");

                DataSupply = DataSupplier.GetSupply(DataDependency);
            }

            public SuppliedDependency(Tuple<IDataSupplier, IDataDependency> tuple)
                : this(tuple.Item1, tuple.Item2)
            {
            }
        }

        #endregion
    }
}
