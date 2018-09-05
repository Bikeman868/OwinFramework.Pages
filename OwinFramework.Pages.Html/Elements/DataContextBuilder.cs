using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Elements
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
        private readonly List<Tuple<IDataSupplier, IDataDependency>> _requiredSuppliers;
        private readonly List<IDataConsumer> _dataConsumers = new List<IDataConsumer>();

        private IDataSupply[] _staticSupplies;
        private List<IDataSupply> _actualDataSupplies;
        private List<Tuple<IDataSupplier, IDataDependency>> _actualDataSuppliers;

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

            _requiredSuppliers = dataScopeRules.SuppliedDependencies == null
                ? new List<Tuple<IDataSupplier, IDataDependency>>()
                : dataScopeRules.SuppliedDependencies.ToList();
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
            _actualDataSupplies = new List<IDataSupply>();
            _actualDataSuppliers = new List<Tuple<IDataSupplier, IDataDependency>>();

            foreach (var supply in _requiredDataSupplies) AddSupply(supply);
            foreach (var supplier in _requiredSuppliers) AddDataSupplier(supplier);
            foreach (var consumer in _dataConsumers) Resolve(consumer);

            if (_children != null)
            {
                for (var i = 0; i < _children.Length; i++)
                    _children[i].ResolveSupplies();
            }

            var sortedSuppliers = GetSuppliedDependenciesOrdered();

            // TODO: Wire up dynamic supplies

            foreach (var dataSupplier in sortedSuppliers)
            {
                var supplier = dataSupplier.Item1;
                var dependency = dataSupplier.Item2;

                if (dependency != null && !supplier.IsSupplierOf(dependency))
                    throw new Exception("Supplier '" + supplier + "' is not a supplier of '" + dependency + "'");

                var supply = supplier.GetSupply(dependency);
                AddSupply(supply);
            }

            _staticSupplies = _actualDataSupplies.Where(s => s.IsStatic).ToArray();
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
            if (_actualDataSupplies.All(s => s != supply))
            {
                _actualDataSupplies.Add(supply);
                Resolve(supply as IDataConsumer);
            }
        }

        /// <summary>
        /// Adds a data supplier and specific type of data to supply to this
        /// context without duplicating data in the same scope
        /// </summary>
        private void AddDataSupplier(Tuple<IDataSupplier, IDataDependency> dataSupplier)
        {
            if (_actualDataSuppliers.All(s => !Equals(s.Item2, dataSupplier.Item2)))
            {
                _actualDataSuppliers.Add(dataSupplier);
                Resolve(dataSupplier.Item1 as IDataConsumer);
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
                    Resolve(dataSupplier, true);
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

                AddDataSupplier(new Tuple<IDataSupplier, IDataDependency>(supplier, dependency));
            }
            else
            {
                _parent.Resolve(dependency);
            }
        }

        private bool Resolve(Tuple<IDataSupplier, IDataDependency> dataSupplier, bool addIfMissing)
        {
            var dependency = dataSupplier.Item2;

            if (IsSupplierOf(dependency)) return true;

            if (_parent != null && _parent.Resolve(dataSupplier, false))
                return true;

            if (addIfMissing)
            {
                if (IsInScope(dependency))
                {
                    AddDataSupplier(dataSupplier);
                    var supplier = dataSupplier.Item1;
                    Resolve(supplier as IDataConsumer);
                    return true;
                }
                return _parent.Resolve(dataSupplier, true);
            }
            return false;
        }

        /// <summary>
        /// Returns true if this instance is already supplying this type of data
        /// </summary>
        private bool IsSupplierOf(IDataDependency dependency)
        {
            if (_actualDataSuppliers == null) return false;
            return _actualDataSuppliers.Any(supplier => Equals(supplier.Item2, dependency));
        }

        private List<Tuple<IDataSupplier, IDataDependency>> GetSuppliedDependenciesOrdered()
        {
            // TODO: Order supplies by their dependency graph

            /*
            Func<Tuple<IDataSupplier, IDataDependency>, Tuple<IDataSupplier, IDataDependency>, bool> isDependentOn = (d1, d2) =>
            {
                if (d1.DependentSupplies == null || d1.DependentSupplies.Count == 0) return false;
                if (ReferenceEquals(d2.Supply, null)) return false;
                return d1.DependentSupplies.Any(s => ReferenceEquals(s, d2.Supply));
            };
            var listSorter = new DependencyListSorter<SuppliedDependency>();

            lock (_suppliedDependencies)
                return listSorter.Sort(_suppliedDependencies, isDependentOn);
             */

            return _actualDataSuppliers;
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
    }
}
