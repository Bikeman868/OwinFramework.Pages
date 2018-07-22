using System;
using System.Collections;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Elements;

namespace OwinFramework.Pages.Html.Runtime
{
    /// <summary>
    /// Base implementation of IRegion. Inheriting from this class will insulate you
    /// from any future additions to the IRegion interface.
    /// You can also use this class directly but it provides only minimal region 
    /// functionallity
    /// </summary>
    public class Region : Element, IRegion, IDataScopeProvider, IDataSupplier, IDataSupply
    {
        private readonly IRegionDependenciesFactory _regionDependenciesFactory;
        public override ElementType ElementType { get { return ElementType.Region; } }
        public bool IsInstance { get { return false; } }

        public IElement Content { get; protected set; }

        public Action<IHtmlWriter> WriteOpen { get; set; }
        public Action<IHtmlWriter> WriteClose { get; set; }
        public Action<IHtmlWriter> WriteChildOpen { get; set; }
        public Action<IHtmlWriter> WriteChildClose { get; set; }

        private Type _repeatType;
        private Type _listType;

        public string RepeatScope { get; set; }

        public string ListScope { get; set; }

        public Type ListType { get { return _listType; } }

        public Type RepeatType
        {
            get { return _repeatType; }
            set
            {
                _repeatType = value;
                _listType = typeof(IList<>).MakeGenericType(value);
            }
        }

        #region Construction and initialization

        /// <summary>
        /// Do not change this constructor signature, it will break application
        /// classes that inherit from this class. Add dependencies to
        /// IRegionDependenciesFactory and IRegionDependencies
        /// </summary>
        public Region(IRegionDependenciesFactory dependencies)
            : base(dependencies.DataConsumerFactory)
        {
            // DO NOT change the method signature of this constructor as
            // this would break all regions in all applications that use
            // this framework!!

            _regionDependenciesFactory = dependencies;

            WriteOpen = w => { };
            WriteClose = w => { };
            WriteChildOpen = w => { };
            WriteChildClose = w => { };

            _dataScopeProvider = dependencies.DataScopeProviderFactory.Create();
        }

        #endregion

        #region Debug info

        protected override DebugInfo PopulateDebugInfo(DebugInfo debugInfo)
        {
            var debugRegion = debugInfo as DebugRegion ?? new DebugRegion();

            debugRegion.Content = Content == null ? null : Content.GetDebugInfo();
            debugRegion.RepeatScope = RepeatScope;
            debugRegion.RepeatType = _repeatType;
            debugRegion.ListType = _listType;
            debugRegion.ListScope = ListScope;

            return base.PopulateDebugInfo(debugRegion);
        }

        #endregion

        #region Setting up and wiring

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

        #endregion

        #region Methods called by region instances to render content

        protected IDataContext SelectDataContext(IRenderContext context, IDataScopeProvider scope)
        {
            var data = context.Data;
            context.SelectDataContext(scope.Id);
            return data;
        }

        public virtual IWriteResult WriteHead(
            IRenderContext context, 
            IDataScopeProvider scope,
            bool includeChildren)
        {
            var savedData = SelectDataContext(context, scope);
            var result = base.WriteHead(context, includeChildren);
            context.Data = savedData;

            return result;
        }

        public virtual IWriteResult WriteHtml(
            IRenderContext context,
            IDataScopeProvider scope,
            IElement content)
        {
            var result = WriteResult.Continue();
            var savedData = SelectDataContext(context, scope);

            WriteOpen(context.Html);

            if (content != null)
            {
                if (_repeatType == null)
                {
                    result.Add(content.WriteHtml(context));
                }
                else
                {
                    var list = (IEnumerable)context.Data.Get(_listType, ListScope);
                    if (list != null)
                    {
                        foreach (var item in list)
                        {
                            context.Data.Set(_repeatType, item, RepeatScope);
                            Supply(context, context.Data);
                            WriteChildOpen(context.Html);
                            result.Add(content.WriteHtml(context));
                            WriteChildClose(context.Html);
                        }
                    }
                }
            }

            WriteClose(context.Html);
            context.Data = savedData;
            return result;
        }

        public virtual IWriteResult WriteInitializationScript(
            IRenderContext context,
            IDataScopeProvider scope,
            bool includeChildren)
        {
            var savedData = SelectDataContext(context, scope);
            var result = base.WriteInitializationScript(context, includeChildren);
            context.Data = savedData;

            return result;
        }

        public virtual IWriteResult WriteTitle(
            IRenderContext context,
            IDataScopeProvider scope,
            bool includeChildren)
        {
            var savedData = SelectDataContext(context, scope);
            var result = base.WriteTitle(context, includeChildren);
            context.Data = savedData;

            return result;
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

        IDataSupply IDataScopeProvider.AddSupplier(IDataSupplier supplier, IDataDependency dependency)
        {
            return _dataScopeProvider.AddSupplier(supplier, dependency);
        }

        void IDataScopeProvider.AddSupply(IDataSupply supply)
        {
            _dataScopeProvider.AddSupply(supply);
        }

        void IDataScopeProvider.AddConsumer(IDataConsumer consumer)
        {
            _dataScopeProvider.AddConsumer(consumer);
        }

        #endregion

        #region IDataSupplier

        IList<Type> IDataSupplier.SuppliedTypes
        {
            get
            {
                return _repeatType == null 
                    ? new List<Type>() 
                    : new List<Type> { _repeatType };
            }
        }

        IDataDependency IDataSupplier.DefaultDependency 
        { 
            get
            {
                return RepeatType == null
                    ? null 
                    : _regionDependenciesFactory.DataDependencyFactory.Create(RepeatType, RepeatScope);
            }
        }

        bool IDataSupplier.IsScoped
        {
            get { return !string.IsNullOrEmpty(RepeatScope); }
        }

        void IDataSupplier.Add(
            IDataDependency dependency, 
            Action<IRenderContext, IDataContext, IDataDependency> action,
            bool isStatic)
        {
            throw new NotImplementedException();
        }

        bool IDataSupplier.IsSupplierOf(IDataDependency dependency)
        {
            if (_repeatType == null) return false;
            if (dependency.DataType == null) return false;
            if (_repeatType != dependency.DataType) return false;
            if (string.IsNullOrEmpty(RepeatScope)) return string.IsNullOrEmpty(dependency.ScopeName);
            return string.Equals(RepeatScope, dependency.ScopeName, StringComparison.OrdinalIgnoreCase);
        }

        IDataSupply IDataSupplier.GetSupply(IDataDependency dependency)
        {
            return this;
        }

        bool IDataSupply.IsStatic { get { return false; } }

        public void Supply(IRenderContext renderContext, IDataContext dataContext)
        {
            if (OnDataSupplied != null)
            {
                var args = new DataSuppliedEventArgs
                {
                    RenderContext = renderContext,
                    DataContext = dataContext
                };
                OnDataSupplied(this, args);
            }
        }

        public event EventHandler<DataSuppliedEventArgs> OnDataSupplied;

        #endregion
    }
}
