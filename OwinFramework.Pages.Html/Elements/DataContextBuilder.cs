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

        private List<IDataScope> _dataScopes;
        private readonly List<IDataSupply> _requiredDataSupplies;
        private readonly List<Tuple<IDataSupplier, IDataDependency>> _requiredSuppliers;
        private readonly List<IDataConsumer> _dataConsumers = new List<IDataConsumer>();

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
            if (_children != null)
            {
                for(var i = 0; i < _children.Length; i++)
                {
                    _children[i].ResolveSupplies();
                }
            }

            var dataSupplies = new List<IDataSupply>();
            var dataSuppliers = new List<Tuple<IDataSupplier, IDataDependency>>();

            Action<IDataSupply> addDataSupplies = supply =>
            {
                if (!dataSupplies.Any(s => ReferenceEquals(s, supply)))
                    dataSupplies.Add(supply);
            };

            Action<Tuple<IDataSupplier, IDataDependency>> addDataSupplier = supplier =>
            {
                if (!dataSuppliers.Any(s => s.Item1 == supplier.Item1 && s.Item2 == supplier.Item2))
                    dataSuppliers.Add(supplier);
            };

            foreach (var supply in _requiredDataSupplies) addDataSupplies(supply);
            foreach (var supplier in _requiredSuppliers) addDataSupplier(supplier);

            foreach (var consumer in _dataConsumers)
            {
                var needs = consumer.GetConsumerNeeds();

                if (needs.DataSupplyDependencies != null)
                    foreach (var supply in needs.DataSupplyDependencies) addDataSupplies(supply);

                if (needs.DataDependencies != null)
                {
                    foreach (var dependency in needs.DataDependencies)
                    {
                        var supplier = _dataCatalog.FindSupplier(dependency);
                        if (supplier == null)
                            throw new Exception("The data catalog does not contain a supplier of '" + dependency + "'");

                        addDataSupplier(new Tuple<IDataSupplier, IDataDependency>(supplier, dependency));
                    }
                }

                if (needs.DataProviderDependencies != null)
                {
                    
                    foreach(var dataProvider in needs.DataProviderDependencies)
                    {
                        var provider = dataProvider.Item1;
                        var dependency = dataProvider.Item2;
                        addDataSupplier(new Tuple<IDataSupplier, IDataDependency>(provider, dependency));
                    }
                }
            }

            foreach(var dataSupplier in dataSuppliers)
            {
                var supplier = dataSupplier.Item1;
                var dependency = dataSupplier.Item2;

                if (dependency != null && !supplier.IsSupplierOf(dependency))
                    throw new Exception("Supplier '" + supplier + "' is not a supplier of '" + dependency + "'");

                var supply = supplier.GetSupply(dependency);
                addDataSupplies(supply);
            }

            // TODO: Order supplies by their dependency graph
            // TODO: Wire up dynamic supplies

            _staticSupplies = dataSupplies.Where(s => s.IsStatic).ToArray();
        }

        public void SetupDataContext(IRenderContext renderContext)
        {
            var rootContext = _dataContextFactory.Create(renderContext, this);
            AddDataContext(renderContext, rootContext);
            renderContext.SelectDataContext(Id);
        }

        public void BuildDataContextTree(IRenderContext renderContext, IDataContext parentDataContext)
        {
            var dataContext = parentDataContext.CreateChild(this);
            AddDataContext(renderContext, dataContext);
        }

        public bool IsInScope(IDataDependency dependency)
        {
            return false;
        }

        public void AddMissingData(IRenderContext renderContext, IDataDependency missingDependency)
        {
        }

        private void AddDataContext(IRenderContext renderContext, IDataContext dataContext)
        {
            for (var i = 0; i < _staticSupplies.Length; i++)
                _staticSupplies[i].Supply(renderContext, dataContext);

            renderContext.AddDataContext(Id, dataContext);

            if (_children != null)
            {
                foreach (var child in _children)
                    child.BuildDataContextTree(renderContext, dataContext);
            }
        }
    }
}
