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

        public override string ToString()
        {
            if (_repeatType == null) 
                return "'" + Name + "' region that does not repeat";

            if (string.IsNullOrEmpty(RepeatScope))
                return "'" + Name + "' region repeating " + _repeatType.DisplayName(TypeExtensions.NamespaceOption.Ending);

            return "'" + Name + "' region repeating '" + RepeatScope + "' " + _repeatType.DisplayName(TypeExtensions.NamespaceOption.Ending);
        }

        #endregion

        #region Setting up and wiring

        public virtual void Populate(IElement content)
        {
            Content = content;
        }

        public IRegion CreateInstance(IElement content)
        {
            return new PageRegion(_regionDependenciesFactory, this, content);
        }

        public override IEnumerator<IElement> GetChildren()
        {
            return Content == null 
                ? null 
                : Content.AsEnumerable().GetEnumerator();
        }

        #endregion

        #region Methods called by region instances to render content

        public virtual IWriteResult WriteHead(
            IRenderContext context, 
            IDataScopeProvider scope,
            bool includeChildren)
        {
            context.Trace(() => ToString() + " writing page head in scope [" + scope + "]" + (includeChildren ? " including children" : ""));

            var savedData = scope.SetDataContext(context);
            var result = base.WriteHead(context, includeChildren);
            context.Data = savedData;

            return result;
        }

        public virtual IWriteResult WriteHtml(
            IRenderContext context,
            IDataScopeProvider scope,
            IElement content)
        {
            context.Trace(() => ToString() + " writing page head in scope [" + scope + "] with content [" + content + "]");

            var result = WriteResult.Continue();
            var savedData = scope.SetDataContext(context);

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
                    if (ReferenceEquals(list, null))
                    {
                        context.Trace(() => ToString() + " will not repeat because there is no list in the data context of the required type");
                    }
                    else
                    {
                        foreach (var item in list)
                        {
                            context.Trace(() => ToString() + " repeating content for next list entry");

                            context.Data.Set(_repeatType, item, RepeatScope);
                            ((IDataSupply)this).Supply(context, context.Data);

                            context.TraceIndent();

                            WriteChildOpen(context.Html);
                            result.Add(content.WriteHtml(context));
                            WriteChildClose(context.Html);

                            context.TraceOutdent();
                        }
                    }
                }
            }

            WriteClose(context.Html);

            context.Trace(() => ToString() + " restoring saved data context");
            context.Data = savedData;

            return result;
        }

        public virtual IWriteResult WriteInitializationScript(
            IRenderContext context,
            IDataScopeProvider scope,
            bool includeChildren)
        {
            context.Trace(() => ToString() + " writing initialization script in scope [" + scope + "]" + (includeChildren ? " including children" : ""));

            var savedData = scope.SetDataContext(context);
            var result = base.WriteInitializationScript(context, includeChildren);
            context.Data = savedData;

            return result;
        }

        public virtual IWriteResult WriteTitle(
            IRenderContext context,
            IDataScopeProvider scope,
            bool includeChildren)
        {
            context.Trace(() => ToString() + " writing page title in scope [" + scope + "]" + (includeChildren ? " including children" : ""));

            var savedData = scope.SetDataContext(context);
            var result = base.WriteTitle(context, includeChildren);
            context.Data = savedData;

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

        IList<IDataSupply> IDataScopeProvider.AddConsumer(IDataConsumer consumer)
        {
            return _dataScopeProvider.AddConsumer(consumer);
        }

        IDataContext IDataScopeProvider.SetDataContext(IRenderContext renderContext)
        {
            return _dataScopeProvider.SetDataContext(renderContext);
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

        DebugDataSupplier IDataSupplier.GetDebugInfo()
        {
            var debugInfo = new DebugDataSupplier
            {
                Instance = this,
                Name = Name + " region",
            };

            if (RepeatType != null)
            {
                debugInfo.SuppliedTypes = new List<Type> { RepeatType };

                debugInfo.DefaultSupply = new DebugDataScope
                {
                    DataType = RepeatType,
                    ScopeName = RepeatScope
                };
            }

            return debugInfo;
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
                renderContext.Trace(() => "region triggering dynamic data supply on " + count + " dependents");

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

        DebugDataSupply IDataSupply.GetDebugInfo()
        {
            return new DebugDataSupply
            {
                Instance = this,
                IsStatic = false,
                SubscriberCount = _onSupplyActions.Count,
                SuppliedData = new DebugDataScope
                {
                    DataType = RepeatType,
                    ScopeName = RepeatScope
                },
                Supplier = ((IDataSupplier)this).GetDebugInfo()
            };
        }

        #endregion
    }
}
