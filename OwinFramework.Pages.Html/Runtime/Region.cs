using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
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
            _dataScopeProvider = regionDependenciesFactory.DataScopeProviderFactory.Create(null);
        }

        DebugElement IElement.GetDebugInfo()
        {
            return GetDebugInfo();
        }

        public DebugRegion GetDebugInfo()
        {
            var debugInfo = new DebugRegion
            { 
                Content = Content == null ? null : Content.GetDebugInfo()
            };
            PopulateDebugInfo(debugInfo);
            return debugInfo;
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
            IRenderContext context,
            IElement content)
        {
            return content == null 
                ? WriteResult.Continue() 
                : content.WriteHtml(context);
        }

        #region IDataScopeProvider
        
        private readonly IDataScopeProvider _dataScopeProvider;

        int IDataScopeProvider.Id { get { return _dataScopeProvider.Id; } }

        bool IDataScopeProvider.IsInScope(Type type, string scopeName)
        {
            return _dataScopeProvider.IsInScope(type, scopeName);
        }

        void IDataScopeProvider.SetupDataContext(IRenderContext renderContext)
        {
            _dataScopeProvider.SetupDataContext(renderContext);
        }

        void IDataScopeProvider.AddMissingData(IRenderContext renderContext, IDataDependency missingDependency)
        {
            _dataScopeProvider.AddMissingData(renderContext, missingDependency);
        }

        IDataScopeProvider IDataScopeProvider.Parent
        {
            get { return _dataScopeProvider.Parent; }
        }

        void IDataScopeProvider.AddChild(IDataScopeProvider child)
        {
            _dataScopeProvider.AddChild(child);
        }

        void IDataScopeProvider.SetParent(IDataScopeProvider parent)
        {
            _dataScopeProvider.SetParent(parent);
        }

        void IDataScopeProvider.AddScope(Type type, string scopeName)
        {
            _dataScopeProvider.AddScope(type, scopeName);
        }

        void IDataScopeProvider.Add(IDataProvider dataProvider, IDataDependency dependency)
        {
            _dataScopeProvider.Add(dataProvider, dependency);
        }

        void IDataScopeProvider.Add(IDataDependency dependency)
        {
            _dataScopeProvider.Add(dependency);
        }

        void IDataScopeProvider.ResolveDataProviders(IList<IDataProvider> existingProviders)
        {
            _dataScopeProvider.ResolveDataProviders(existingProviders);
        }

        List<IDataProvider> IDataScopeProvider.DataProviders
        {
            get { return _dataScopeProvider.DataProviders; }
        }

        void IDataScopeProvider.BuildDataContextTree(IRenderContext renderContext, IDataContext dataContext, bool isParentDataContext)
        {
            _dataScopeProvider.BuildDataContextTree(renderContext, dataContext, isParentDataContext);
        }

        #endregion
    }
}
