using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.Builders
{
    /// <summary>
    /// Contains the information needed to construct a heirachy of data contexts
    /// for a request
    /// </summary>
    public class DataContextDefinition
    {
        private readonly IDataContextFactory _dataContextFactory;

        private readonly int _elementId;
        private readonly DataContextDefinition _parent;
        private readonly List<DataContextDefinition> _children = new List<DataContextDefinition>();
        private readonly List<DataReference> _dataReferences = new List<DataReference>();
        private readonly List<ProviderReference> _dataProviders = new List<ProviderReference>();

        public DataContextDefinition(
            IDataContextFactory dataContextFactory, 
            int elementId)
        {
            _dataContextFactory = dataContextFactory;
            _elementId = elementId;
        }

        private DataContextDefinition(
            IDataContextFactory dataContextFactory,
            int elementId,
            DataContextDefinition parent)
        {
            _dataContextFactory = dataContextFactory;
            _elementId = elementId;
            _parent = parent;
        }

        public List<DataReference> DataReferences { get { return _dataReferences; } }

        public void AddDataReference(Type type, string scope)
        {
            if (_dataReferences.Any(dr => dr.Type == type && string.Equals(dr.Scope, scope)))
                return;

            _dataReferences.Add(new DataReference
                {
                    Type = type,
                    Scope = scope
                });
        }

        public void AddProvider(IDataProvider provider, Type type)
        {
            _dataProviders.Add(new ProviderReference
                {
                    DataProvider = provider,
                    Type = type
                });
        }

        public DataContextDefinition AddChild(int elementId)
        {
            var child = new DataContextDefinition(_dataContextFactory, elementId, this);
            _children.Add(child);
            return child;
        }

        public void BuildDataContextTree(IRenderContext renderContext)
        {
            BuildDataContextTree(renderContext, null);
            renderContext.SelectDataContext(_elementId);
        }

        private void BuildDataContextTree(IRenderContext renderContext, IDataContext parentDataContext)
        {
            var dataContext = parentDataContext == null 
                ? _dataContextFactory.Create(renderContext) 
                : parentDataContext.CreateChild();
            renderContext.AddDataContext(_elementId, dataContext);

            foreach (var provider in _dataProviders)
                provider.DataProvider.Satisfy(renderContext, dataContext, null);

            foreach (var child in _children)
                child.BuildDataContextTree(renderContext, dataContext);
        }

        private class ProviderReference
        {
            public IDataProvider DataProvider;
            public Type Type;
        }
    }
}
