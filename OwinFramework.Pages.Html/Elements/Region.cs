using System;
using System.Linq;
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
    /// Base implementation of IRegion. Applications inherit from this class 
    /// to insulate their code from any future additions to the IRegion interface
    /// </summary>
    public class Region : Element, IRegion, IDataScopeRules
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

        private readonly IRegionDependenciesFactory _dependencies;

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

            _dependencies = dependencies;

            WriteOpen = w => { };
            WriteClose = w => { };
            WriteChildOpen = w => { };
            WriteChildClose = w => { };

            _dataScopeRules = dependencies.DataScopeProviderFactory.Create();
        }

        #endregion

        #region IDataConsumer

        protected override void AddDynamicDataNeeds(DataConsumerNeeds needs)
        {
            if (_listType != null)
                needs.Add(_dependencies.DataDependencyFactory.Create(_listType, ListScope));
        }

        #endregion

        #region Debug info

        protected override T PopulateDebugInfo<T>(DebugInfo debugInfo, int parentDepth, int childDepth)
        {
            if (typeof(T).IsAssignableFrom(typeof(DebugRegion)))
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

                if (_dataScopeRules != null)
                    debugRegion.Scope = _dataScopeRules.GetDebugInfo<DebugDataScopeRules>();

                return base.PopulateDebugInfo<T>(debugRegion, parentDepth, childDepth);
            }

            if (typeof(T).IsAssignableFrom(typeof(DebugDataScopeRules)))
            {
                var debugDataScopeRules = _dataScopeRules.GetDebugInfo<DebugDataScopeRules>(parentDepth, childDepth);

                if (RepeatType != null)
                {
                    if (debugDataScopeRules.DataSupplies == null)
                        debugDataScopeRules.DataSupplies = new List<DebugSuppliedDependency>();

                    var repeatDataSupply = new DebugSuppliedDependency
                    {
                        DataTypeSupplied = new DebugDataScope 
                        { 
                            DataType = RepeatType,
                            ScopeName = RepeatScope
                        },
                        DataConsumer = new DebugDataConsumer 
                        {
                            DependentData = new List<DebugDataScope>
                            {
                                new DebugDataScope
                                {
                                    DataType = ListType,
                                    ScopeName = ListScope
                                }
                            }
                        }
                    };

                    debugDataScopeRules.DataSupplies.Add(repeatDataSupply);
                }

                return base.PopulateDebugInfo<T>(debugDataScopeRules, parentDepth, childDepth);
            }

            if (typeof(T).IsAssignableFrom(typeof(DebugDataSupplier)))
            {
                var debugDataSupplier = debugInfo as DebugDataSupplier ?? new DebugDataSupplier();

                if (RepeatType != null)
                {
                    debugDataSupplier.SuppliedTypes = new List<Type> { RepeatType };
                    debugDataSupplier.DefaultSupply = new DebugDataScope
                    {
                        DataType = RepeatType,
                        ScopeName = RepeatScope
                    };
                }

                return base.PopulateDebugInfo<T>(debugDataSupplier, parentDepth, childDepth);
            }

            return base.PopulateDebugInfo<T>(debugInfo, parentDepth, childDepth);
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
            PageArea pageArea,
            Action<IRenderContext, object> onListItem,
            Func<IRenderContext, PageArea, IWriteResult> contentWriter)
        {
#if TRACE
            context.Trace(() => ToString() + " writing page " + Enum.GetName(typeof(PageArea), pageArea).ToLower());
#endif
            IWriteResult result;

            if (context.IncludeComments)
                context.Html.WriteComment("region " + Name);

            if (pageArea == PageArea.Body)
                WriteOpen(context.Html);

            if (pageArea == PageArea.Body && !ReferenceEquals(_repeatType, null))
            {
                result = WriteResult.Continue();

#if TRACE
                context.Trace(() => ToString() + " getting " + (string.IsNullOrEmpty(ListScope) ? "" : ListScope + " ") + _listType.DisplayName(TypeExtensions.NamespaceOption.None) + " from " + context.Data);
#endif
                var list = context.Data.Get(_listType, ListScope) as IEnumerable;
                if (!ReferenceEquals(list, null))
                {
                    foreach (var item in list)
                    {
#if TRACE
                        context.Trace(() => ToString() + " setting next " + (string.IsNullOrEmpty(RepeatScope) ? "" : RepeatScope + " ") + _repeatType.DisplayName(TypeExtensions.NamespaceOption.None) + " in " + context.Data + " " + item);
#endif

                        context.Data.Set(_repeatType, item, RepeatScope);
                        
                        if (!ReferenceEquals(onListItem, null))
                            onListItem(context, item);

                        context.TraceIndent();

                        WriteChildOpen(context.Html);
                        result.Add(contentWriter(context, pageArea));
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
                result = contentWriter(context, pageArea);
            }

            if (pageArea == PageArea.Body)
                WriteClose(context.Html);

            return result;
        }

        #endregion

        #region IDataScopeRules Mixin

        private readonly IDataScopeRules _dataScopeRules;

        string IDataScopeRules.ElementName
        {
            get { return _dataScopeRules.ElementName; }
            set { _dataScopeRules.ElementName = value; }
        }

        void IDataScopeRules.AddScope(Type type, string scopeName)
        {
            _dataScopeRules.AddScope(type, scopeName);
        }

        void IDataScopeRules.AddSupplier(IDataSupplier supplier, IDataDependency dependency)
        {
            _dataScopeRules.AddSupplier(supplier, dependency);
        }

        void IDataScopeRules.AddSupply(IDataSupply supply)
        {
            _dataScopeRules.AddSupply(supply);
        }

        IList<IDataScope> IDataScopeRules.DataScopes
        {
            get 
            {
                var configuredScopes = _dataScopeRules.DataScopes;

                if (string.IsNullOrEmpty(RepeatScope))
                    return configuredScopes;

                var scopes = configuredScopes.ToList();
                scopes.Add(_dependencies.DataScopeFactory.Create(RepeatType, RepeatScope));

                return scopes;
            }
        }

        IList<Tuple<IDataSupplier, IDataDependency>> IDataScopeRules.SuppliedDependencies
        {
            get { return _dataScopeRules.SuppliedDependencies; }
        }

        IList<IDataSupply> IDataScopeRules.DataSupplies
        {
            get { return _dataScopeRules.DataSupplies; }
        }

        #endregion
    }
}
