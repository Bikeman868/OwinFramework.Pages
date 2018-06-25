using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class ClonedRegion : ClonedElement<IRegion>, IRegion
    {
        public ElementType ElementType { get { return ElementType.Region; } }

        private readonly IRegionDependenciesFactory _dependenciesFactory;
        private IElement _content;

        public ClonedRegion(
            IRegionDependenciesFactory dependenciesFactory, 
            IRegion parent, 
            IElement content)
            : base(parent)
        {
            _dependenciesFactory = dependenciesFactory;
            _content = content;
        }

        public void Populate(IElement content)
        {
            _content = content;
        }

        public IRegion Clone(IElement content)
        {
            return new ClonedRegion(_dependenciesFactory, Parent, content);
        }

        public override IEnumerator<IElement> GetChildren()
        {
            return _content == null ? null : _content.AsEnumerable().GetEnumerator();
        }

        public IWriteResult WriteHtml(IRenderContext renderContext, bool includeChildren)
        {
            return Parent.WriteHtml(renderContext, includeChildren ? _content : null);
        }

        public IWriteResult WriteHtml(IRenderContext renderContext, IElement content)
        {
            return Parent.WriteHtml(renderContext, content);
        }
        
        #region IDataScopeProvider

        private readonly IDataScopeProvider _dataScopeProvider;

        int IDataScopeProvider.Id { get { return _dataScopeProvider.Id; } }
        IList<IDataScope> IDataScopeProvider.DataScopes { get { return _dataScopeProvider.DataScopes; } }
        IDataContextDefinition IDataScopeProvider.DataContextDefinition { get { return _dataScopeProvider.DataContextDefinition; } }

        IDataScopeProvider IDataScopeProvider.Parent
        {
            get { return _dataScopeProvider.Parent; }
            set { _dataScopeProvider.Parent = value; }
        }

        bool IDataScopeProvider.Provides(Type type, string scopeName)
        {
            return _dataScopeProvider.Provides(type, scopeName);
        }

        void IDataScopeProvider.ResolveDataScopes()
        {
            _dataScopeProvider.ResolveDataScopes();
        }

        void IDataScopeProvider.SetupDataContext(IRenderContext renderContext)
        {
            _dataScopeProvider.SetupDataContext(renderContext);
        }

        void IDataScopeProvider.AddMissingData(IRenderContext renderContext, IDataDependency missingDependency)
        {
            _dataScopeProvider.AddMissingData(renderContext, missingDependency);
        }

        void IDataScopeProvider.AddChild(IDataScopeProvider child)
        {
            _dataScopeProvider.AddChild(child);
        }

        #endregion
    }
}
