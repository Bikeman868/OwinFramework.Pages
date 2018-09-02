using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Elements
{
    public class DataContextBuilder : IDataContextBuilder
    {
        private readonly IDataContextFactory _dataContextFactory;

        private DataContextBuilder[] _children;
        private DataContextBuilder _parent;

        public DataContextBuilder(
            IDataContextFactory dataContextFactory,
            IDataScopeProvider dataScopeProvider)
        {
            _dataContextFactory = dataContextFactory;
        }

        public IDataContextBuilder AddChild(IDataScopeProvider dataScopeProvider)
        {
            var child = new DataContextBuilder(_dataContextFactory, dataScopeProvider)
            {
                _parent = this
            };

            if (_children == null)
                _children = new[] { child };
            else
                _children = _children.Concat(new[] { child }).ToArray();
        }

        public IDataSupply AddDependency(IDataDependency dependency)
        {
            throw new System.NotImplementedException();
        }

        public IList<IDataSupply> AddConsumer(IDataConsumer consumer)
        {
            throw new System.NotImplementedException();
        }

        public bool IsInScope(IDataDependency dependency)
        {
            throw new System.NotImplementedException();
        }

        public void AddMissingData(IRenderContext renderContext, IDataDependency missingDependency)
        {
            throw new System.NotImplementedException();
        }
    }
}
