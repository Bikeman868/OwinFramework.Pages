using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Builders;

namespace OwinFramework.Pages.Html.Runtime
{
    /// <summary>
    /// Base implementation of IRegion. Inheriting from this class will insulate you
    /// from any future additions to the IRegion interface.
    /// You can also use this class directly but it provides only minimal region 
    /// functionallity
    /// </summary>
    public class Region : Element, IRegion
    {
        private readonly IRegionDependenciesFactory _regionDependenciesFactory;
        public override ElementType ElementType { get { return ElementType.Region; } }
        public bool IsClone { get { return false; } }

        protected IElement Content;

        /// <summary>
        /// Do not change this constructor signature, it will break application
        /// classes that inherit from this class. Add dependencies to
        /// IRegionDependenciesFactory and IRegionDependencies
        /// </summary>
        public Region(IRegionDependenciesFactory regionDependenciesFactory)
        {
            // DO NOT change the method signature of this constructor as
            // this would break all regions in all applications that use
            // this framework!!

            _regionDependenciesFactory = regionDependenciesFactory;
            _dataScopeProvider = regionDependenciesFactory.DataScopeProviderFactory.Create();
        }

        public virtual void Populate(IElement content)
        {
            Content = content;
        }

        public IRegion Clone(IElement content)
        {
            return new ClonedRegion(_regionDependenciesFactory, this, content);
        }

        public override IEnumerator<IElement> GetChildren()
        {
            return Content == null 
                ? null 
                : Content.AsEnumerable().GetEnumerator();
        }

        public virtual IWriteResult WriteHtml(
            IRenderContext renderContext,
            IElement content)
        {
            return content == null 
                ? WriteResult.Continue() 
                : content.WriteHtml(renderContext);
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
