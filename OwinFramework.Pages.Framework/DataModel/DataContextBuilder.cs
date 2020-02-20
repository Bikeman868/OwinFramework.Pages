#if DEBUG
#define DETAILED_TRACE
#endif


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Framework.Utility;

namespace OwinFramework.Pages.Framework.DataModel
{
    public class DataContextBuilder : IDataContextBuilder, IDebuggable
    {
        public int Id { get; private set; }

        private readonly IDataContextFactory _dataContextFactory;
        private readonly IIdManager _idManager;
        private readonly IDataCatalog _dataCatalog;

        /// <summary>
        /// If this is a child data context then this points to the parent
        /// </summary>
        private DataContextBuilder _parent;


        /// <summary>
        /// The children of this data context. If children can not resolve data
        /// needs and the required data is not in scope for the child then the
        /// child will defer to this parent etc back up the tree.
        /// </summary>
        private DataContextBuilder[] _children;

        /// <summary>
        /// Defines what data is in scope for this context. When dats is needed
        /// that is in scope it will be resolved at this level. If the required
        /// data is not in scope then this context will defer to its parent.
        /// </summary>
        private readonly List<IDataScope> _dataScopes;

        /// <summary>
        /// The required data supplies are the data that was directly requested by
        /// the application. This does not include implied or transitory data dependencies
        /// </summary>
        private readonly List<IDataSupply> _scopeDataSupplies;

        /// <summary>
        /// A list of required dependencies and the suppliers that have been identified
        /// to supply each of these requirements. These are provided by the data scope rules
        /// (page or region that establishes a new data scope)
        /// </summary>
        private readonly List<SuppliedDependency> _scopeSuppliedDependencies;

        /// <summary>
        /// Data consumers are the things that have data needs that must be met. These
        /// things can be elements on the page or data suppliers that derive their
        /// supplied data from other data suppliers
        /// </summary>
        private readonly List<IDataConsumer> _dataConsumers = new List<IDataConsumer>();

        /// <summary>
        /// These are additional data supplies that were discovered by examining the
        /// data needs of data supplies identified by the data scope
        /// </summary>
        private readonly List<IDataSupply> _additionalSupplies = new List<IDataSupply>();

        /// <summary>
        /// This is a complete list of all the data dependencies and the suppliers that
        /// will supply each dependency. These are sorted so that dependents are executed
        /// before the things that they depend on and this sorted list is stored as the
        /// list of supplies to execute on every page request.
        /// </summary>
        private readonly List<SuppliedDependency> _suppliedDependencies = new List<SuppliedDependency>();

        /// <summary>
        /// These data supplies are executed on every page request in the
        /// order that thay are listed in this array. The array is sorted according
        /// to the supplier dependencies so that supplies are only executed after all
        /// of the supplies that they depend on.
        /// Note that this is an array because Lists are not thread safe and
        /// this list is enumerated by request processing threads
        /// </summary>
        private IDataSupply[] _orderedSupplies;

        /// <summary>
        /// Constructs a builder that can build data contexts that are populated
        /// with all of the data needed according to a set of data scope rules
        /// </summary>
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

            var dataScopes = dataScopeRules.DataScopes;
            _dataScopes = dataScopes == null 
                ? new List<IDataScope>()
                : dataScopes.ToList();

            var dataSupplies = dataScopeRules.DataSupplies;
            _scopeDataSupplies = dataSupplies == null
                ? new List<IDataSupply>()
                : dataSupplies.ToList();

            var suppliedDependencies = dataScopeRules.SuppliedDependencies;
            _scopeSuppliedDependencies = suppliedDependencies == null
                ? new List<SuppliedDependency>()
                : suppliedDependencies.Select(sd => new SuppliedDependency(sd)).ToList();

#if DETAILED_TRACE
            Trace.WriteLine("Data context builder #" + Id + " constructing from data scope rules " + dataScopeRules);

            if (_dataScopes.Count > 0)
            {
                Trace.WriteLine("Data context builder #" + Id + " scopes:");
                foreach (var s in _dataScopes) Trace.WriteLine("    " + s);
            }

            if (_scopeDataSupplies.Count > 0)
            {
                Trace.WriteLine("Data context builder #" + Id + " data supplies:");
                foreach (var s in _scopeDataSupplies) Trace.WriteLine("    " + s);
            }

            if (_scopeSuppliedDependencies.Count > 0)
            {
                Trace.WriteLine("Data context builder #" + Id + " supplied dependencies:");
                foreach (var s in _scopeSuppliedDependencies) Trace.WriteLine("    " + s);
            }
#endif
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

#if DETAILED_TRACE
            Trace.WriteLine("Data context builder #" + Id + " is the parent of #" + child.Id);
#endif

            if (_children == null)
                _children = new[] { child };
            else
                _children = _children.Concat(new[] { child }).ToArray();

            return child;
        }

        public void AddConsumer(IDataConsumer consumer)
        {
#if DETAILED_TRACE
            Trace.WriteLine("Data context builder #" + Id + " adding consumer " + consumer);
#endif
            _dataConsumers.Add(consumer);
        }

        public bool IsInScope(IDataDependency dependency)
        {
            if (_parent == null) return true;
            if (_dataScopes == null) return false;
            return _dataScopes.Any(scope => scope.IsMatch(dependency));
        }

        public void AddMissingData(IRenderContext renderContext, IDataDependency missingDependency)
        {
#if DEBUG
            renderContext.Trace(() => "Data context builder #" + Id + " has been notified of a missing dependency on " + missingDependency);
#endif

            ResolveDataDependencySupplies(missingDependency);

            var root = this;
            while (!ReferenceEquals(root._parent, null))
                root = root._parent;

            ResolveSupplies();

            renderContext.DeleteDataContextTree();
            root.SetupDataContext(renderContext);
        }

        #region Maintaining unique lists of supplies and suppliers

        /// <summary>
        /// Adds a data supply to this context without adding the same supply twice
        /// </summary>
        private void AddSupply(IDataSupply supply)
        {
            if (_scopeDataSupplies.Any(s => s == supply))
                return;

            if (_additionalSupplies.Any(s => s == supply))
                return;

#if DEBUG
            Trace.WriteLine("Data context builder #" + Id + " adding supply '" + supply + "'");
#endif
            _additionalSupplies.Add(supply);

            // If this supply is also a consumer of data then add it to the consumer list
            if (supply is IDataConsumer dataConsumer)
                AddConsumer(dataConsumer);
        }

        /// <summary>
        /// Adds a data supplier and specific type of data to supply to this
        /// context without duplicating data in the same scope
        /// </summary>
        private void AddSuppliedDependency(SuppliedDependency suppliedDependency)
        {
            if (_suppliedDependencies.Any(s => Equals(s.DataDependency, suppliedDependency.DataDependency)))
                return;

            if (suppliedDependency.DataDependency == null && 
                _suppliedDependencies.Any(s => Equals(s.DataSupplier, suppliedDependency.DataSupplier)))
                return;

#if DEBUG
            Trace.WriteLine("Data context builder #" + Id + " adding supplied dependency '" + suppliedDependency + "'");
#endif
            _suppliedDependencies.Add(suppliedDependency);
        }

        #endregion

        #region Resolving data needs

        public void ResolveSupplies()
        {
#if DEBUG
            Trace.WriteLine("Data context builder #" + Id + " resolving suppliers");
#endif

            // The scope supplies and suppliers are the ones that were directly
            // configured for this element in the application.

            foreach (var supply in _scopeDataSupplies) AddSuppliedDependency(new SuppliedDependency(supply));
            foreach (var supplier in _scopeSuppliedDependencies) AddSuppliedDependency(supplier);

            // The next section of code walks down the dependency chains adding
            // all of the suppliers and consumers (which may have other suppliers etc)
            // These resolving activities can result in new supplies or suppliers being
            // added to the supply chain.

            var additionalSuppliesIndex = 0;
            var suppliedDependencyIndex = 0;
            var consumerIndex = 0;

            void RecursivelyResolveDataSuppliers()
            {
                var done = false;
                while (!done)
                {
                    done = true;

                    for (; additionalSuppliesIndex < _additionalSupplies.Count; additionalSuppliesIndex++)
                    {
                        AddSuppliedDependency(new SuppliedDependency(_additionalSupplies[additionalSuppliesIndex]));
                    }

                    for (; suppliedDependencyIndex < _suppliedDependencies.Count; suppliedDependencyIndex++)
                    {
                        ResolveSuppliedDependencyNeeds(_suppliedDependencies[suppliedDependencyIndex]);
                        done = false;
                    }

                    for (; consumerIndex < _dataConsumers.Count; consumerIndex++)
                    {
                        ResolveConsumerNeeds(_dataConsumers[consumerIndex]);
                        done = false;
                    }
                }
            }

            RecursivelyResolveDataSuppliers();

            // Now that we know all of the data we are going to supply, the child
            // data needs can be resolved. The children will defer out of scope
            // data resolution to us and if we can not resolve it then we pass
            // it to our parent etc.

            if (_children != null)
            {
                for (var i = 0; i < _children.Length; i++)
                    _children[i].ResolveSupplies();
            }

            // Resolving the children might have added new dependencies and suppliers.
            // At this point the list is finalized and can be sorted into the proper
            // execution order to ensure all dependents run before their dependencies.

            RecursivelyResolveDataSuppliers();
            var sortedList = GetSuppliedDependenciesInExecutionOrder();

            // Static supplies are the ones that are executed once only for 
            // each render context to supply data that does not change during the
            // rendering process. There are also dynamic supplies that change at
            // the page is rendered (repeating regions for example).

            _orderedSupplies = sortedList
                .Where(sd => sd.DataSupply != null && sd.DataSupply.IsStatic)
                .Select(sd => sd.DataSupply)
                .ToArray();
        }

        /// <summary>
        /// Resolves all of the data needs of a supplied dependency and updates
        /// the DependentSupplies property with a list of the supplies it depends on.
        /// If any dependant supplies are dynamic then this supply also becomes dynamic
        /// </summary>
        private void ResolveSuppliedDependencyNeeds(SuppliedDependency dependency)
        {
            var dataConsumer = dependency.DataSupplier as IDataConsumer;
            if (dataConsumer == null) return;

            var dependentSupplies = ResolveConsumerNeeds(dataConsumer);
            if (dependentSupplies == null) return;

            dependency.DependentSupplies = dependentSupplies;

            foreach (var dynamicSupply in dependentSupplies.Where(s => !s.IsStatic))
            {
                dependency.DataSupply.IsStatic = false;
                dynamicSupply.AddOnSupplyAction(rc =>
                {
                    var dc = rc.GetDataContext(Id);
                    dependency.DataSupply.Supply(rc, dc);
                });
            }
        }

        /// <summary>
        /// Resolves all of the data needs of a data consumer and returns a list of the data
        /// supplies that it depends on
        /// </summary>
        private List<IDataSupply> ResolveConsumerNeeds(IDataConsumer consumer)
        {
            if (consumer == null) return null;

            var needs = consumer.GetConsumerNeeds();
            if (needs == null) return null;

            var dataDependencies = needs.DataDependencies;
            var dataSupplyDependencies = needs.DataSupplyDependencies;
            var dataSupplierDependencies = needs.DataSupplierDependencies;

#if DETAILED_TRACE
            Trace.WriteLine("Data context builder #" + Id + " resolving consumer needs for " + consumer);

            if (dataSupplyDependencies != null && dataSupplyDependencies.Count > 0)
            {
                Trace.WriteLine("  consumer needs data supplies:");
                foreach (var supply in dataSupplyDependencies)
                    Trace.WriteLine("    " + supply);
            }

            if (dataSupplierDependencies != null && dataSupplierDependencies.Count > 0)
            {
                Trace.WriteLine("  consumer needs suppliers:");
                foreach (var supplierDependency in dataSupplierDependencies)
                    Trace.WriteLine("    " + supplierDependency);
            }

            if (dataDependencies != null && dataDependencies.Count > 0)
            {
                Trace.WriteLine("  consumer needs data:");
                foreach (var dependency in dataDependencies)
                    Trace.WriteLine("    " + dependency);
            }
#endif

            var dependentSupplies = new List<IDataSupply>();

            if (dataSupplyDependencies != null)
            {
                foreach (var supply in dataSupplyDependencies)
                {
                    AddSupply(supply);
                    dependentSupplies.Add(supply);
                }
            }

            if (dataSupplierDependencies != null)
            {
                foreach (var dataSupplier in dataSupplierDependencies)
                {
                    var supplies = ResolveDataSupplierSupplies(dataSupplier.Item1, dataSupplier.Item2, true);
                    if (supplies != null) dependentSupplies.AddRange(supplies);
                }
            }

            if (dataDependencies != null)
            {
                foreach (var dependency in dataDependencies)
                {
                    var supplies = ResolveDataDependencySupplies(dependency);
                    if (supplies != null) dependentSupplies.AddRange(supplies);
                }
            }
            
#if DETAILED_TRACE
            if (dependentSupplies.Count > 0)
            {
                Trace.WriteLine("  consumer depends on supplies:");
                foreach (var supply in dependentSupplies)
                    Trace.WriteLine("    " + supply);
            }
#endif
            return dependentSupplies;
        }

        private List<IDataSupply> ResolveDataDependencySupplies(IDataDependency dependency)
        {
#if DETAILED_TRACE
            Trace.WriteLine("Data context builder #" + Id + " resolving dependency '" + dependency + "'");
#endif
            var suppliedDependency = _suppliedDependencies == null 
                ? null
                : _suppliedDependencies.FirstOrDefault(sd => Equals(sd.DataDependency, dependency));

            if (suppliedDependency != null)
            {
#if DETAILED_TRACE
                Trace.WriteLine("Data context builder #" + Id + " dependency '" + dependency + "' already has a supplier");
#endif
                return new List<IDataSupply> { suppliedDependency.DataSupply };
            }

            if (IsInScope(dependency))
            {
#if DETAILED_TRACE
                Trace.WriteLine("Data context builder #" + Id + " dependency '" + dependency + "' is in scope, getting from data catalog");
#endif
                var supplier = _dataCatalog.FindSupplier(dependency);
                if (supplier == null)
                    throw new Exception("The data catalog does not contain a supplier of '" + dependency + "'");

                suppliedDependency = new SuppliedDependency(supplier, dependency);
                AddSuppliedDependency(suppliedDependency);
                return new List<IDataSupply> { suppliedDependency.DataSupply };
            }

#if DETAILED_TRACE
            Trace.WriteLine("Data context builder #" + Id + " dependency '" + dependency + "' is not in scope, passing to parent to resolve");
#endif
            return _parent.ResolveDataDependencySupplies(dependency);
        }

        private List<IDataSupply> ResolveDataSupplierSupplies(IDataSupplier supplier, IDataDependency dataToSupply, bool addIfMissing)
        {
#if DETAILED_TRACE
            Trace.WriteLine("Data context builder #" + Id + (addIfMissing ? " adding '" : " locating '") + dataToSupply + "' from' " + supplier + "'");
#endif
            var suppliedDependency = _suppliedDependencies == null
                ? null
                : _suppliedDependencies.FirstOrDefault(sd => Equals(sd.DataDependency, dataToSupply));

            if (suppliedDependency != null)
            {
#if DETAILED_TRACE
                Trace.WriteLine("Data context builder #" + Id + " dependency '" + dataToSupply + "' already has a supplier");
#endif
                return new List<IDataSupply> { suppliedDependency.DataSupply };
            }

            if (_parent != null)
            {
                var dependentSupplies = _parent.ResolveDataSupplierSupplies(supplier, dataToSupply, false);
                if (dependentSupplies != null)
                {
#if DETAILED_TRACE
                    Trace.WriteLine("Data context builder #" + Id + " parent was able to resolve the dependency");
#endif
                    return dependentSupplies;
                }
            }

            if (!addIfMissing) return null;

#if DETAILED_TRACE
            Trace.WriteLine("Data context builder #" + Id + " dependency on '" + dataToSupply + "' must be added");
#endif
            suppliedDependency = new SuppliedDependency(supplier, dataToSupply);
            AddSuppliedDependency(suppliedDependency);

            ResolveConsumerNeeds(supplier as IDataConsumer);

            return new List<IDataSupply> { suppliedDependency.DataSupply };
        }

        #endregion

        #region Ordering suppliers so satify dependencies

        /// <summary>
        /// Returns a list of the supplied dependencies sorted such that all list entries
        /// appear after all of the entries that they depend on
        /// </summary>
        private IEnumerable<SuppliedDependency> GetSuppliedDependenciesInExecutionOrder()
        {
            if (_suppliedDependencies == null || _suppliedDependencies.Count == 0)
                return Enumerable.Empty<SuppliedDependency>();

            Func<SuppliedDependency, SuppliedDependency, bool> isDependentOn = (d1, d2) =>
            {
                if (d1.DependentSupplies == null || d1.DependentSupplies.Count == 0) return false;
                if (ReferenceEquals(d2.DataSupply, null)) return false;
                return d1.DependentSupplies.Any(s => ReferenceEquals(s, d2.DataSupply));
            };
            var listSorter = new DependencyListSorter<SuppliedDependency>();

            lock (_suppliedDependencies)
            {
#if DETAILED_TRACE
                Trace.WriteLine("Data context builder #" + Id + " sorting dependencies. Original order:");
                foreach (var d in _suppliedDependencies) Trace.WriteLine("    " + d);
#endif
                var sortedList = listSorter.Sort(_suppliedDependencies, isDependentOn);
#if DETAILED_TRACE
                Trace.WriteLine("Data context builder #" + Id + " Order after sorting:");
                foreach (var d in sortedList) Trace.WriteLine("    " + d);
#endif
                return sortedList;
            }
        }

        #endregion

        #region Building the data context for a request

        public void SetupDataContext(IRenderContext renderContext)
        {
            var rootContext = _dataContextFactory.Create(renderContext, this);
            AddDataContext(renderContext, rootContext);
            renderContext.SelectDataContext(Id);
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
            if (_orderedSupplies != null)
            {
                for (var i = 0; i < _orderedSupplies.Length; i++)
                    _orderedSupplies[i].Supply(renderContext, dataContext);
            }

            renderContext.AddDataContext(Id, dataContext);

            if (_children != null)
            {
                for (var i = 0; i < _children.Length; i++ )
                    _children[i].BuildDataContextTree(renderContext, dataContext);
            }
        }

        #endregion

        #region Debug info

        T IDebuggable.GetDebugInfo<T>(int parentDepth, int childDepth)
        {
            var debugInfo = new DebugDataScopeRules
            {
                Instance = this,
                Name = "Data context builder #" + Id,
                Type = "Data context builder"
            };

            if (_dataScopes != null && _dataScopes.Count > 0)
            {
                debugInfo.Scopes = _dataScopes
                    .Select(s => new DebugDataScope
                        {
                            DataType = s.DataType,
                            ScopeName = s.ScopeName
                        })
                    .ToList();
            }

            if (_suppliedDependencies != null && _suppliedDependencies.Count > 0)
            {
                debugInfo.DataSupplies = _suppliedDependencies
                    .Select(ds => new DebugSuppliedDependency
                        {
                            Supplier = ds.DataSupplier.GetDebugInfo<DebugDataSupplier>(),
                            DataSupply = ds.DataSupply.GetDebugInfo<DebugDataSupply>(),
                            DataTypeSupplied = ds.DataDependency == null ? null : ds.DataDependency.GetDebugInfo<DebugDataScope>()
                        })
                    .ToList();
            }

            //if (_dataSupplies != null && _dataSupplies.Count > 0)
            //{
            //}

            if (_parent != null && parentDepth != 0)
                debugInfo.Parent = _parent.GetDebugInfo<T>(parentDepth - 1, 0);

            if (_children != null && _children.Length > 0 && childDepth != 0)
                debugInfo.Children = _children
                    .Select(c => c.GetDebugInfo<T>())
                    .Cast<DebugInfo>()
                    .ToList();

            return debugInfo as T;
        }

        #endregion

        #region SuppliedDependency class

        private class SuppliedDependency
        {
            /// <summary>
            /// This is the data provider or region that built the data supply.
            /// This can be null if we don't know who the supplier is
            /// </summary>
            public readonly IDataSupplier DataSupplier;

            /// <summary>
            /// This defines the type of data we asked the data supplier to supply.
            /// This can be null if the supply was added by the application without
            /// specifying the type of data that it was supplying
            /// </summary>
            public readonly IDataDependency DataDependency;

            /// <summary>
            /// This is the object that will add data to the data context during
            /// page rendering operations. This can be null initially before the
            /// data supply has been obtained from the data supplier.
            /// </summary>
            public readonly IDataSupply DataSupply;

            /// <summary>
            /// These supplies provide data that this supply depends on and therefore
            /// must be run prior to this one. This property is populated after the
            /// data needs have been resolved into supplies
            /// </summary>
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

            public SuppliedDependency(IDataSupply dataSupply, IDataDependency dataDependency = null)
            {
                DataSupply = dataSupply;
                DataDependency = dataDependency;
                DataSupplier = null;
                DependentSupplies = new List<IDataSupply>();

                if (dataSupply == null)
                    throw new Exception("You cannot have a " + GetType().Name + " that supplies no data");
            }

            public override string ToString()
            {
                if (DataSupplier == null)
                {
                    if (DataDependency == null) return DataSupply.ToString();
                    return DataSupply + " -> " + DataDependency;
                }

                if (DataDependency == null) return DataSupplier.ToString();
                return DataSupplier + " -> " + DataDependency;
            }
        }

        #endregion
    }
}
