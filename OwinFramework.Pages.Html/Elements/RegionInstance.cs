using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Elements
{
    internal class RegionInstance : ElementInstance<IRegion>, IRegion, IDataScopeProvider
    {
        public override ElementType ElementType { get { return ElementType.Region; } }

        public string RepeatScope 
        { 
            get { return Parent.RepeatScope; }
            set { throw new InvalidOperationException("You can not set the repeat scope on a region instance"); }
         }

        public Type RepeatType
        {
            get { return Parent.RepeatType; }
            set { throw new InvalidOperationException("You can not set the repeat type on region instance"); }
        }

        public string ListScope
        {
            get { return Parent.ListScope; }
            set { throw new InvalidOperationException("You can not set the list scope on a region instance"); }
        }

        public Type ListType
        {
            get { return Parent.ListType; }
        }

        private readonly IRegionDependenciesFactory _dependenciesFactory;
        private IElement _content;

        public RegionInstance(
            IRegionDependenciesFactory dependenciesFactory, 
            IRegion parent, 
            IElement content)
            : base(parent)
        {
            _dependenciesFactory = dependenciesFactory;

            var parentDataScope = parent as IDataScopeProvider;
            _dataScopeProvider = parentDataScope == null 
                ? dependenciesFactory.DataScopeProviderFactory.Create()
                : parentDataScope.CreateInstance();
            _dataScopeProvider.ElementName = "Region instance " + parent.Name;

            content = content ?? parent.Content;

            var layout = content as ILayout;
            var region = content as IRegion;

            _content = layout == null 
                ? (region == null 
                    ? content 
                    : region.CreateInstance(null)) 
                : layout.CreateInstance();
        }

        public override void Initialize(IInitializationData initializationData)
        {
            initializationData.Push();
            initializationData.AddScope(_dataScopeProvider);
            base.Initialize(initializationData);
            initializationData.Pop();
        }

        protected override DebugInfo PopulateDebugInfo(DebugInfo debugInfo)
        {
            var parentDebugInfo = Parent == null ? null : (DebugRegion)Parent.GetDebugInfo();

            _dataScopeProvider.ElementName = "Region instance " + Name;

            var debugRegion = debugInfo as DebugRegion ?? new DebugRegion();

            debugRegion.Type = "Instance of region";
            debugRegion.Content = _content == null ? null : _content.GetDebugInfo();
            debugRegion.InstanceOf = parentDebugInfo;
            debugRegion.Scope = _dataScopeProvider.GetDebugInfo(-1, 1);
            debugRegion.RepeatScope = RepeatScope;
            debugRegion.RepeatType = RepeatType;
            debugRegion.ListType = ListType;
            debugRegion.ListScope = ListScope;

            return base.PopulateDebugInfo(debugRegion);
        }

        public IElement Content { get { return _content; } }

        public void Populate(IElement content)
        {
            _content = content;
        }

        public IRegion CreateInstance(IElement content)
        {
            return new RegionInstance(_dependenciesFactory, Parent, content ?? _content);
        }

        public override IEnumerator<IElement> GetChildren()
        {
            return _content == null ? null : _content.AsEnumerable().GetEnumerator();
        }

        #region IElement rendering methods

        public override IWriteResult WriteHead(IRenderContext context, bool includeChildren)
        {
            return Parent.WriteHead(context, _dataScopeProvider, includeChildren);
        }

        public override IWriteResult WriteHtml(IRenderContext context, bool includeChildren)
        {
            return Parent.WriteHtml(context, _dataScopeProvider, includeChildren ? _content : null);
        }

        public override IWriteResult WriteInitializationScript(IRenderContext context, bool includeChildren)
        {
            return Parent.WriteInitializationScript(context, _dataScopeProvider, includeChildren);
        }

        public override IWriteResult WriteTitle(IRenderContext context, bool includeChildren)
        {
            return Parent.WriteTitle(context, _dataScopeProvider, includeChildren);
        }

        #endregion

        #region IRegion specific rendering methods

        public IWriteResult WriteHead(IRenderContext context, IDataScopeProvider scope, bool includeChildren)
        {
            return Parent.WriteHead(context, scope, includeChildren);
        }

        public IWriteResult WriteHtml(IRenderContext context, IDataScopeProvider scope, IElement content)
        {
            return Parent.WriteHtml(context, scope, content);
        }

        public IWriteResult WriteInitializationScript(IRenderContext context, IDataScopeProvider scope, bool includeChildren)
        {
            return Parent.WriteInitializationScript(context, scope, includeChildren);
        }

        public IWriteResult WriteTitle(IRenderContext context, IDataScopeProvider scope, bool includeChildren)
        {
            return Parent.WriteTitle(context, scope, includeChildren);
        }

        #endregion

        #region IDataScopeProvider

        private readonly IDataScopeProvider _dataScopeProvider;

        int IDataScopeProvider.Id { get { return _dataScopeProvider.Id; } }

        string IDataScopeProvider.ElementName
        {
            get { return _dataScopeProvider.ElementName; }
            set { _dataScopeProvider.ElementName = value; }
        }

        DebugDataScopeProvider IDataScopeProvider.GetDebugInfo(int parentDepth, int childDepth)
        {
            return _dataScopeProvider.GetDebugInfo(parentDepth, childDepth);
        }

        bool IDataScopeProvider.IsInScope(IDataDependency dependency)
        {
            return _dataScopeProvider.IsInScope(dependency);
        }

        void IDataScopeProvider.SetupDataContext(IRenderContext renderContext)
        {
            _dataScopeProvider.SetupDataContext(renderContext);
        }

        IDataScopeProvider IDataScopeProvider.CreateInstance()
        {
            return _dataScopeProvider.CreateInstance();
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

        void IDataScopeProvider.Initialize(IDataScopeProvider parent)
        {
            _dataScopeProvider.Initialize(parent);
        }

        void IDataScopeProvider.AddScope(Type type, string scopeName)
        {
            _dataScopeProvider.AddScope(type, scopeName);
        }

        IDataSupply IDataScopeProvider.AddDependency(IDataDependency dependency)
        {
            return _dataScopeProvider.AddDependency(dependency);
        }

        void IDataScopeProvider.BuildDataContextTree(IRenderContext renderContext, IDataContext parentDataContext)
        {
            _dataScopeProvider.BuildDataContextTree(renderContext, parentDataContext);
        }

        IDataSupply IDataScopeProvider.AddSupplier(IDataSupplier supplier, IDataDependency dependency, IList<IDataSupply> supplyDependencies)
        {
            return _dataScopeProvider.AddSupplier(supplier, dependency, supplyDependencies);
        }

        void IDataScopeProvider.AddSupply(IDataSupply supply)
        {
            _dataScopeProvider.AddSupply(supply);
        }

        IList<IDataSupply> IDataScopeProvider.AddConsumer(IDataConsumer consumer)
        {
            return _dataScopeProvider.AddConsumer(consumer);
        }

        #endregion
    }
}
