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
        public bool IsInstance { get { return false; } }

        public IElement Content { get; protected set; }

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

        public override void Initialize(IInitializationData initializationData)
        {
            initializationData.Push();
            initializationData.AddScope(_dataScopeProvider);
            base.Initialize(initializationData);
            initializationData.Pop();
        }

        DebugElement IElement.GetDebugInfo()
        {
            return GetDebugInfo();
        }

        public DebugRegion GetDebugInfo()
        {
            var debugInfo = new DebugRegion
            { 
                Content = Content == null ? null : Content.GetDebugInfo(),
                Scope = _dataScopeProvider.GetDebugInfo(-1, 2)
            };
            PopulateDebugInfo(debugInfo);
            return debugInfo;
        }

        public virtual void Populate(IElement content)
        {
            Content = content;
        }

        public IRegion CreateInstance(IElement content)
        {
            return new RegionInstance(_regionDependenciesFactory, this, content);
        }

        public override IEnumerator<IElement> GetChildren()
        {
            return Content == null 
                ? null 
                : Content.AsEnumerable().GetEnumerator();
        }

        protected IDataContext SelectDataContext(IRenderContext context)
        {
            var data = context.Data;
            context.SelectDataContext(_dataScopeProvider.Id);
            return data;
        }

        public virtual IWriteResult WriteHtml(
            IRenderContext context,
            IElement content)
        {
            if (content == null) return WriteResult.Continue();

            var savedData = SelectDataContext(context);
            var result = content.WriteHtml(context);
            context.Data = savedData;

            return result;
        }

        public override IWriteResult WriteHead(IRenderContext context, bool includeChildren)
        {
            var savedData = SelectDataContext(context);
            var result = base.WriteHead(context, includeChildren);
            context.Data = savedData;

            return result;
        }

        public override IWriteResult WriteHtml(IRenderContext context, bool includeChildren)
        {
            return WriteHtml(context, includeChildren ? Content : null);
        }

        public override IWriteResult WriteInitializationScript(IRenderContext context, bool includeChildren)
        {
            var savedData = SelectDataContext(context);
            var result = base.WriteInitializationScript(context, includeChildren);
            context.Data = savedData;

            return result;
        }

        public override IWriteResult WriteTitle(IRenderContext context, bool includeChildren)
        {
            var savedData = SelectDataContext(context);
            var result = base.WriteInitializationScript(context, includeChildren);
            context.Data = savedData;

            return result;
        }

        #region IDataScopeProvider
        
        private readonly IDataScopeProvider _dataScopeProvider;

        int IDataScopeProvider.Id { get { return _dataScopeProvider.Id; } }

        DebugDataScopeProvider IDataScopeProvider.GetDebugInfo(int parentDepth, int childDepth)
        {
            return _dataScopeProvider.GetDebugInfo(parentDepth, childDepth);
        }

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
