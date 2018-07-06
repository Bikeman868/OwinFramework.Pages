using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class RegionInstance : ElementInstance<IRegion>, IRegion
    {
        public ElementType ElementType { get { return ElementType.Region; } }

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
            _dataScopeProvider = parent.Clone();

            content = content ?? parent.Content;

            var layout = content as ILayout;
            var region = content as IRegion;

            _content = layout == null 
                ? (region == null ? content 
                : region.CreateInstance(null)) : layout.CreateInstance();
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
            var parentDebugInfo = Parent == null ? null : Parent.GetDebugInfo();

            _dataScopeProvider.ElementName = "Region " + Name;

            var debugInfo = new DebugRegion
            {
                Type = "Instance of region",
                Content = _content == null ? null : _content.GetDebugInfo(),
                InstanceOf = parentDebugInfo,
                Scope = _dataScopeProvider.GetDebugInfo(-1, 1),
                RepeatScope = RepeatScope,
                RepeatType = RepeatType,
                ListType = ListType,
                ListScope = ListScope
            };
            PopulateDebugInfo(debugInfo);
            return debugInfo;
        }

        public IElement Content { get { return _content; } }

        public void Populate(IElement content)
        {
            _content = content;
        }

        public IRegion CreateInstance(IElement content)
        {
            return new RegionInstance(_dependenciesFactory, Parent, content);
        }

        public override IEnumerator<IElement> GetChildren()
        {
            return _content == null ? null : _content.AsEnumerable().GetEnumerator();
        }

        #region IElement rendering methods

        public new IWriteResult WriteHead(IRenderContext context, bool includeChildren)
        {
            return Parent.WriteHead(context, _dataScopeProvider, includeChildren);
        }

        public IWriteResult WriteHtml(IRenderContext context, bool includeChildren)
        {
            return Parent.WriteHtml(context, _dataScopeProvider, includeChildren ? _content : null);
        }

        public new IWriteResult WriteInitializationScript(IRenderContext context, bool includeChildren)
        {
            return Parent.WriteInitializationScript(context, _dataScopeProvider, includeChildren);
        }

        public new IWriteResult WriteTitle(IRenderContext context, bool includeChildren)
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

        IDataScopeProvider IDataScopeProvider.Clone()
        {
            return _dataScopeProvider.Clone();
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

        void IDataScopeProvider.AddElementScope(Type type, string scopeName)
        {
            _dataScopeProvider.AddElementScope(type, scopeName);
        }

        IDataSupply IDataScopeProvider.Add(IDataDependency dependency)
        {
            return _dataScopeProvider.Add(dependency);
        }

        void IDataScopeProvider.BuildDataContextTree(IRenderContext renderContext, IDataContext parentDataContext)
        {
            _dataScopeProvider.BuildDataContextTree(renderContext, parentDataContext);
        }

        #endregion
    }
}
