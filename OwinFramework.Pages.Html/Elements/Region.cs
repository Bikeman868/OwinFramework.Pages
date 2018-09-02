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
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Elements
{
    /// <summary>
    /// Base implementation of IRegion. Applications inherit from this olass 
    /// to insulate their code from any future additions to the IRegion interface
    /// </summary>
    public class Region : Element, IRegion, IDataScopeProvider, IDataSupplier, IDataSupply
    {
        public override ElementType ElementType { get { return ElementType.Region; } }

        public IElement Content { get; set; }

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

        private IRegionDependenciesFactory _regionDependenciesFactory;

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

        protected override DebugInfo PopulateDebugInfo(DebugInfo debugInfo, int parentDepth, int childDepth)
        {
            var debugRegion = debugInfo as DebugRegion ?? new DebugRegion();

            if (childDepth != 0)
            {
                var content = Content as IDebuggable;
                if (content != null)
                    debugRegion.Children = new List<DebugInfo> { content.GetDebugInfo(0, childDepth - 1) };
            }

            debugRegion.RepeatScope = RepeatScope;
            debugRegion.RepeatType = _repeatType;
            debugRegion.ListType = _listType;
            debugRegion.ListScope = ListScope;

            debugRegion.DataSupply = new DebugDataSupply
            {
                Instance = this,
                IsStatic = false,
                SubscriberCount = _onSupplyActions.Count,
                SuppliedData = new DebugDataScope
                {
                    DataType = RepeatType,
                    ScopeName = RepeatScope
                },
                Supplier = new DebugDataSupplier
                {
                    Instance = this,
                    Name = Name + " region",
                }
            };

            if (RepeatType != null)
            {
                debugRegion.DataSupply.Supplier.SuppliedTypes = new List<Type> { RepeatType };

                debugRegion.DataSupply.Supplier.DefaultSupply = new DebugDataScope
                {
                    DataType = RepeatType,
                    ScopeName = RepeatScope
                };
            }

            return base.PopulateDebugInfo(debugRegion, parentDepth, childDepth);
        }

        public override string ToString()
        {
            if (_repeatType == null) 
                return "'" + Name + "' region that does not repeat";

            if (string.IsNullOrEmpty(RepeatScope))
                return "'" + Name + "' region repeating " + _repeatType.DisplayName(TypeExtensions.NamespaceOption.Ending);

            return "'" + Name + "' region repeating '" + RepeatScope + "' " + _repeatType.DisplayName(TypeExtensions.NamespaceOption.Ending);
        }

        #endregion

        #region Writing html to the page

        public virtual IWriteResult WritePageArea(
            IRenderContext context, 
            IDataContextBuilder dataContextBuilder, 
            PageArea pageArea, 
            Action<object> onListItem,
            Func<IRenderContext, IDataContextBuilder, PageArea, IWriteResult> contentWriter)
        {
#if TRACE
            context.Trace(() => ToString() + " writing page " + Enum.GetName(typeof(PageArea), pageArea).ToLower());
#endif
            IWriteResult result;

            WriteOpen(context.Html);

            if (pageArea == PageArea.Body && !ReferenceEquals(_repeatType, null))
            {
                result = WriteResult.Continue();

                var list = context.Data.Get(_listType, ListScope) as IEnumerable;
                if (!ReferenceEquals(list, null))
                {
                    foreach (var item in list)
                    {
#if TRACE
                        context.Trace(() => ToString() + " repeating content for next list entry");
#endif

                        context.Data.Set(_repeatType, item, RepeatScope);

                        if (!ReferenceEquals(onListItem, null))
                            onListItem(item);

                        context.TraceIndent();

                        WriteChildOpen(context.Html);
                        result.Add(contentWriter(context, dataContextBuilder, pageArea));
                        WriteChildClose(context.Html);

                        context.TraceOutdent();
                    }
                }
#if TRACE
                else
                {
                    context.Trace(() => ToString() + " will not repeat because there is no list in the data context of the required type");
                }
#endif
            }
            else
            {
                result = contentWriter(context, dataContextBuilder, pageArea);
            }

            WriteClose(context.Html);

            return result;
        }

        #endregion

        #region IDataScopeProvider MixIn

        private readonly IDataScopeProvider _dataScopeProvider;

        int IDataScopeProvider.Id { get { return _dataScopeProvider.Id; } }

        string IDataScopeProvider.ElementName
        {
            get { return _dataScopeProvider.ElementName; }
            set { _dataScopeProvider.ElementName = value; }
        }

        void IDataScopeProvider.SetupDataContext(IRenderContext renderContext)
        {
            _dataScopeProvider.SetupDataContext(renderContext);
        }

        IDataScopeProvider IDataScopeProvider.CreateInstance()
        {
            return _dataScopeProvider.CreateInstance();
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

        IDataContext IDataScopeProvider.SetDataContext(IRenderContext renderContext)
        {
            return _dataScopeProvider.SetDataContext(renderContext);
        }

        #endregion

        #region IDataContextBuilder MixIn

        bool IDataContextBuilder.IsInScope(IDataDependency dependency)
        {
            return _dataScopeProvider.IsInScope(dependency);
        }

        void IDataContextBuilder.AddMissingData(IRenderContext renderContext, IDataDependency missingDependency)
        {
            _dataScopeProvider.AddMissingData(renderContext, missingDependency);
        }

        IDataSupply IDataContextBuilder.AddDependency(IDataDependency dependency)
        {
            return _dataScopeProvider.AddDependency(dependency);
        }

        IList<IDataSupply> IDataContextBuilder.AddConsumer(IDataConsumer consumer)
        {
            return _dataScopeProvider.AddConsumer(consumer);
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
            Action<IRenderContext, IDataContext, IDataDependency> action)
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

        #endregion

        #region IDataSupply

        private readonly List<Action<IRenderContext>> _onSupplyActions = new List<Action<IRenderContext>>();

        bool IDataSupply.IsStatic { get { return false; } set { } }

        void IDataSupply.Supply(IRenderContext renderContext, IDataContext dataContext)
        {
            int count;
            lock (_onSupplyActions) count = _onSupplyActions.Count;

            if (count > 0)
            {
#if TRACE
                renderContext.Trace(() => "region triggering dynamic data supply on " + count + " dependents");
#endif
                renderContext.TraceIndent();
                for (var i = 0; i < count; i++)
                {
                    Action<IRenderContext> action;
                    lock (_onSupplyActions) action = _onSupplyActions[i];
                    action(renderContext);
                }
                renderContext.TraceOutdent();
            }
        }

        void IDataSupply.AddOnSupplyAction(Action<IRenderContext> dataSupplyAction)
        {
            lock (_onSupplyActions) _onSupplyActions.Add(dataSupplyAction);
        }

        #endregion
    }
}
