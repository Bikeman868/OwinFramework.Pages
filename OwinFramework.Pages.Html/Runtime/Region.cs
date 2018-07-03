﻿using System;
using System.Collections;
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

        public Action<IHtmlWriter> WriteOpen { get; set; }
        public Action<IHtmlWriter> WriteClose { get; set; }
        public Action<IHtmlWriter> WriteChildOpen { get; set; }
        public Action<IHtmlWriter> WriteChildClose { get; set; }

        private Type _repeatType;
        private Type _listType;

        public string RepeatScope { get; set; }

        public Type RepeatType
        {
            get { return _repeatType; }
            set
            {
                _repeatType = value;
                _listType = typeof(IList<>).MakeGenericType(value);
            }
        }

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
            _dataScopeProvider = dependencies.DataScopeProviderFactory.Create();

            WriteOpen = w => { };
            WriteClose = w => { };
            WriteChildOpen = w => { };
            WriteChildClose = w => { };
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
                Scope = _dataScopeProvider.GetDebugInfo(-1, 2),
                RepeatScope = RepeatScope,
                RepeatType = RepeatType
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
            var result = WriteResult.Continue();
            var savedData = SelectDataContext(context);

            WriteOpen(context.Html);

            if (content != null)
            {
                if (_repeatType == null)
                {
                    result.Add(content.WriteHtml(context));
                }
                else
                {
                    var list = (IEnumerable)context.Data.Get(_listType, RepeatScope);
                    foreach (var item in list)
                    {
                        context.Data.Set(_repeatType, item);
                        WriteChildOpen(context.Html);
                        result.Add(content.WriteHtml(context));
                        WriteChildClose(context.Html);
                    }
                }
            }

            WriteClose(context.Html);
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

        string IDataScopeProvider.ElementName
        { 
            get { return _dataScopeProvider.ElementName; } 
            set { _dataScopeProvider.ElementName  = value; } 
        }

        IDataScopeProvider IDataScopeProvider.Clone()
        {
            return _dataScopeProvider.Clone();
        }

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

        void IDataScopeProvider.ElementIsProvider(Type type, string scopeName)
        {
            _dataScopeProvider.ElementIsProvider(type, scopeName);
        }

        void IDataScopeProvider.Add(IDataProviderDefinition dataProviderDefinition)
        {
            _dataScopeProvider.Add(dataProviderDefinition);
        }

        bool IDataScopeProvider.CanSatisfyDependency(IDataDependency dependency)
        {
            return _dataScopeProvider.CanSatisfyDependency(dependency);
        }

        void IDataScopeProvider.Add(IDataDependency dependency)
        {
            _dataScopeProvider.Add(dependency);
        }

        void IDataScopeProvider.ResolveDataProviders(IList<IDataProviderDefinition> existingProviders)
        {
            _dataScopeProvider.ResolveDataProviders(existingProviders);
        }

        List<IDataProviderDefinition> IDataScopeProvider.DataProviders
        {
            get { return _dataScopeProvider.DataProviders; }
        }

        void IDataScopeProvider.BuildDataContextTree(IRenderContext renderContext, IDataContext parentDataContext)
        {
            _dataScopeProvider.BuildDataContextTree(renderContext, parentDataContext);
        }

        #endregion
    }
}
