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

        private DataContextBuilder[] _children;
        private DataContextBuilder _parent;

        public DataContextBuilder(
            IDataContextFactory dataContextFactory,
            IIdManager idManager,
            IDataScopeRules dataScopeRules)
        {
            _dataContextFactory = dataContextFactory;
            _idManager = idManager;

            Id = idManager.GetUniqueId();

            // TODO: Extract dependencies from data scope rules
            var scopes = dataScopeRules.DataScopes;
            var supplies = dataScopeRules.DataSupplies;
            var suppliers = dataScopeRules.SuppliedDependencies;
        }

        public IDataContextBuilder AddChild(IDataScopeRules dataScopeRules)
        {
            var child = new DataContextBuilder(_dataContextFactory, _idManager, dataScopeRules)
            {
                _parent = this
            };

            if (_children == null)
                _children = new[] { child };
            else
                _children = _children.Concat(new[] { child }).ToArray();

            return child;
        }

        public IDataSupply AddDependency(IDataDependency dependency)
        {
            return null;
        }

        public IList<IDataSupply> AddConsumer(IDataConsumer consumer)
        {
            return null;
        }

        public void SetupDataContext(IRenderContext renderContext)
        {
            var rootContext = _dataContextFactory.Create(renderContext, this);
            renderContext.AddDataContext(Id, rootContext);

            if (_children != null)
            {
                foreach (var child in _children)
                    child.BuildDataContextTree(renderContext, rootContext);
            }

            renderContext.SelectDataContext(Id);
        }

        public void BuildDataContextTree(IRenderContext renderContext, IDataContext parentDataContext)
        {
            var dataContext = parentDataContext.CreateChild(this);
            renderContext.AddDataContext(Id, dataContext);

            if (_children != null)
            {
                foreach (var child in _children)
                    child.BuildDataContextTree(renderContext, dataContext);
            }
        }

        public bool IsInScope(IDataDependency dependency)
        {
            return false;
        }

        public void AddMissingData(IRenderContext renderContext, IDataDependency missingDependency)
        {
        }
    }
}
