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

            foreach (var consumer in _dataConsumers)
            {
                var needs = consumer.GetConsumerNeeds();

                if (needs.DataSupplyDependencies != null)
                    foreach (var supply in needs.DataSupplyDependencies) AddSupply(supply);

                if (needs.DataDependencies != null)
                {
                    foreach (var dependency in needs.DataDependencies)
                        Resolve(dependency);
                }

                if (needs.DataSupplierDependencies != null)
                {
                    foreach(var dataSupplier in needs.DataSupplierDependencies)
                        Resolve(dataSupplier);
                }
            }

            if (_children != null)
            {
                for (var i = 0; i < _children.Length; i++)
                {
                    _children[i].ResolveSupplies();
                }
            }
            
            foreach (var dataSupplier in _actualDataSuppliers)
            {
                var supplier = dataSupplier.Item1;
                var dependency = dataSupplier.Item2;

                if (dependency != null && !supplier.IsSupplierOf(dependency))
                    throw new Exception("Supplier '" + supplier + "' is not a supplier of '" + dependency + "'");

                var supply = supplier.GetSupply(dependency);
                AddSupply(supply);
            }

            // TODO: Order supplies by their dependency graph
            // TODO: Wire up dynamic supplies

            _staticSupplies = _actualDataSupplies./*Where(s => s.IsStatic).*/ToArray();
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
        }

        #region private implementation details

        /// <summary>
        /// Adds a data supply to this context without adding the same supply twice
        /// </summary>
        private void AddSupply(IDataSupply supply)
        {
            if (_actualDataSupplies.All(s => s != supply))
                _actualDataSupplies.Add(supply);
        }

        /// <summary>
        /// Adds a data supplier and specific type of data to supply to this
        /// context without duplicating data in the same scope
        /// </summary>
        private void AddDataSupplier(Tuple<IDataSupplier, IDataDependency> supplier)
        {
            if (_actualDataSuppliers.All(s => !Equals(s.Item2, supplier.Item2)))
                _actualDataSuppliers.Add(supplier);
        }

        /// <summary>
        /// This is called by children to request the parent to resolve a data
        /// need that is not in scope for the child
        /// </summary>
        private void Resolve(Tuple<IDataSupplier, IDataDependency> dataSupplier)
        {
            var dependency = dataSupplier.Item2;

            if (IsSupplierOf(dependency)) return;

            if (IsInScope(dependency))
                AddDataSupplier(dataSupplier);
            else
                _parent.Resolve(dataSupplier);
        }

        /// <summary>
        /// This is called by children to request the parent to resolve a data
        /// need that is not in scope for the child
        /// </summary>
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

        /// <summary>
        /// Returns true if this instance is already supplying this type of data
        /// </summary>
        private bool IsSupplierOf(IDataDependency dependency)
        {
            if (_actualDataSuppliers == null) return false;
            return _actualDataSuppliers.Any(supplier => Equals(supplier.Item2, dependency));
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
