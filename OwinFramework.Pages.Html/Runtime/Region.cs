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
        }

        #endregion

        #region Debug info

        DebugElement IElement.GetDebugInfo()
        {
            return GetDebugInfo();
        }

        public DebugRegion GetDebugInfo()
        {
            var debugInfo = new DebugRegion
            { 
                Content = Content == null ? null : Content.GetDebugInfo(),
                RepeatScope = RepeatScope,
                RepeatType = _repeatType,
                ListType = _listType,
                ListScope = ListScope
            };
            PopulateDebugInfo(debugInfo);
            return debugInfo;
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
                    foreach (var item in list)
                    {
                        context.Data.Set(_repeatType, item, RepeatScope);
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
    }
}
