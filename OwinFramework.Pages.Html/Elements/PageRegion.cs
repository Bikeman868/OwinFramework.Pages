using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Elements
{
    internal class PageRegion : PageElement, IDataSupplier, IDataSupply, IDataScopeRules
    {
        private readonly IDataContextBuilder _dataContextBuilder;
        private readonly Func<IRenderContext, PageArea, IWriteResult> _writeContent;

        private IRegion Region { get { return (IRegion)Element; } }
        private IDataScopeRules ElementDataScopeRules { get { return Element as IDataScopeRules; } }

        public PageRegion(
            PageElementDependencies dependencies,
            PageElement parent,
            IRegion region, 
            IElement content, 
            IPageData pageData)
            : base(dependencies, parent, region, pageData)
        {
            _dataContextBuilder = pageData.BeginAddElement(Element, this);

            content = content ?? region.Content;
            var layout = content as ILayout;
            var component = content as IComponent;

            if (layout != null)
            {
                var pageLayout = new PageLayout(dependencies, this, layout, null, pageData);
                _writeContent = pageLayout.WritePageArea;
                Children = new PageElement[] { pageLayout };
            }
            else if (component != null)
            {
                var pageComponent = new PageComponent(dependencies, this, component, pageData);
                _writeContent = pageComponent.WritePageArea;
                Children = new PageElement[] { pageComponent };
            }
            else
            {
                Children = null;
                _writeContent = (rc, pa) => WriteResult.Continue();
            }

            pageData.EndAddElement(Element);
        }

        protected override T PopulateDebugInfo<T>(DebugInfo debugInfo, int parentDepth, int childDepth)
        {
            if (typeof(T).IsAssignableFrom(typeof(DebugRegion)))
            {
                var debugRegion = debugInfo as DebugRegion ?? new DebugRegion();

                debugRegion.Scope = Element.GetDebugInfo<DebugDataScopeRules>(0, 0);
                debugRegion.DataContext = _dataContextBuilder.GetDebugInfo<DebugDataScopeRules>();

                var region = Element as IRegion;
                if (region != null)
                {
                    debugRegion.DataSupply = new DebugDataSupply
                    {
                        Instance = this,
                        IsStatic = false,
                        SubscriberCount = _onSupplyActions.Count,
                        SuppliedData = new DebugDataScope
                        {
                            DataType = region.RepeatType,
                            ScopeName = region.RepeatScope
                        },
                        Supplier = new DebugDataSupplier
                        {
                            Instance = this,
                            Name = region.Name,
                        }
                    };
                }
            }

            if (typeof(T).IsAssignableFrom(typeof(DebugDataSupply)))
            {
                var debugDataSupply = debugInfo as DebugDataSupply ?? new DebugDataSupply();

                lock (_onSupplyActions) debugDataSupply.SubscriberCount = _onSupplyActions.Count;
                debugDataSupply.IsStatic = false;

                var region = Element as IRegion;
                if (region != null)
                {
                    debugDataSupply.SuppliedData = new DebugDataScope
                    {
                        DataType = region.RepeatType,
                        ScopeName = region.RepeatScope
                    };
                }

                return base.PopulateDebugInfo<T>(debugDataSupply, parentDepth, childDepth);
            }

            return base.PopulateDebugInfo<T>(debugInfo, parentDepth, childDepth);
        }

        protected override IWriteResult WritePageAreaInternal(
            IRenderContext renderContext,
            PageArea pageArea)
        {
            var region = Element as IRegion;
            if (ReferenceEquals(region, null)) return WriteResult.Continue();

            var data = renderContext.Data;
            renderContext.SelectDataContext(_dataContextBuilder.Id);

            var result = region.WritePageArea(renderContext, pageArea, OnListItem, _writeContent);

            renderContext.Data = data;
            return result;
        }

        private void OnListItem(IRenderContext renderContext, object listItem)
        {
            ((IDataSupply)this).Supply(renderContext, null);
        }

        #region IDataSupplier

        IList<Type> IDataSupplier.SuppliedTypes
        {
            get
            {
                var region = Element as IRegion;
                return region == null || region.RepeatType == null
                    ? new List<Type>()
                    : new List<Type> { region.RepeatType };
            }
        }

        IDataDependency IDataSupplier.DefaultDependency
        {
            get
            {
                var region = Element as IRegion;
                return region == null || region.RepeatType == null
                    ? null
                    : Dependencies.DataDependencyFactory.Create(region.RepeatType, region.RepeatScope);
            }
        }

        bool IDataSupplier.IsScoped
        {
            get 
            {
                var region = Element as IRegion;
                return region != null && !string.IsNullOrEmpty(region.RepeatScope); 
            }
        }

        void IDataSupplier.Add(
            IDataDependency dependency,
            Action<IRenderContext, IDataContext, IDataDependency> action)
        {
            throw new NotImplementedException();
        }

        bool IDataSupplier.IsSupplierOf(IDataDependency dependency)
        {
            if (Region.RepeatType == null) return false;
            if (dependency.DataType == null) return false;
            if (Region.RepeatType != dependency.DataType) return false;
            if (string.IsNullOrEmpty(Region.RepeatScope)) return string.IsNullOrEmpty(dependency.ScopeName);
            return string.Equals(Region.RepeatScope, dependency.ScopeName, StringComparison.OrdinalIgnoreCase);
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

        #endregion

        #region IDataScopeRules


        string IDataScopeRules.ElementName
        {
            get { return Region.Name; }
            set { throw new NotImplementedException(); }
        }

        void IDataScopeRules.AddScope(Type type, string scopeName)
        {
            throw new NotImplementedException();
        }

        void IDataScopeRules.AddSupplier(IDataSupplier supplier, IDataDependency dependency)
        {
            throw new NotImplementedException();
        }

        void IDataScopeRules.AddSupply(IDataSupply supply)
        {
            throw new NotImplementedException();
        }

        IList<IDataScope> IDataScopeRules.DataScopes
        {
            get 
            { 
                return ElementDataScopeRules == null 
                    ? new List<IDataScope>()
                    : ElementDataScopeRules.DataScopes; 
            }
        }

        IList<Tuple<IDataSupplier, IDataDependency>> IDataScopeRules.SuppliedDependencies
        {
            get
            {
                var suppliedDependencies = new List<Tuple<IDataSupplier, IDataDependency>>();

                if (Region.RepeatType != null)
                {
                    var dependency = Dependencies.DataDependencyFactory.Create(Region.RepeatType, Region.RepeatScope);
                    suppliedDependencies.Add(new Tuple<IDataSupplier, IDataDependency>(this, dependency));
                }

                if (ElementDataScopeRules != null)
                    suppliedDependencies.AddRange(ElementDataScopeRules.SuppliedDependencies);

                return suppliedDependencies;
            }
        }

        IList<IDataSupply> IDataScopeRules.DataSupplies
        {
            get
            {
                return ElementDataScopeRules == null
                    ? new List<IDataSupply>()
                    : ElementDataScopeRules.DataSupplies;
            }
        }

        #endregion
    }
}
