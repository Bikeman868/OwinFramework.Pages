using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Base implementation of IComponent. Applications inherit from this olass 
    /// to insulate their code from any future additions to the IComponent interface
    /// </summary>
    public class Component : Element, IComponent, ICloneable
    {
        public override ElementType ElementType { get { return ElementType.Component; } }

        protected readonly IComponentDependenciesFactory Dependencies;

        private Action<IRenderContext>[] _headWriters;
        private Action<IRenderContext>[] _scriptWriters;
        private Action<IRenderContext>[] _styleWriters;
        private Action<IRenderContext>[] _bodyWriters;
        private Action<IRenderContext>[] _initializationWriters;

        /// <summary>
        /// These are actions that must execute during the rendering of the page head
        /// </summary>
        public Action<IRenderContext>[] HeadWriters
        {
            get { return _headWriters; }
            set
            {
                _headWriters = value;
                EnsurePageArea(PageArea.Head, value == null);
            }
        }

        /// <summary>
        /// These are actions that must execute during the rendering of the in-page scripts
        /// </summary>
        public Action<IRenderContext>[] ScriptWriters
        {
            get { return _scriptWriters; }
            set
            {
                _scriptWriters = value;
                EnsurePageArea(PageArea.Scripts, value == null);
            }
        }

        /// <summary>
        /// These are actions that must execute during the rendering of the in-page styles
        /// </summary>
        public Action<IRenderContext>[] StyleWriters
        {
            get { return _styleWriters; }
            set
            {
                _styleWriters = value;
                EnsurePageArea(PageArea.Styles, value == null);
            }
        }

        /// <summary>
        /// These are actions that must execute during the rendering of the page body
        /// </summary>
        public Action<IRenderContext>[] BodyWriters
        {
            get { return _bodyWriters; }
            set
            {
                _bodyWriters = value;
                EnsurePageArea(PageArea.Body, value == null);
            }
        }

        /// <summary>
        /// These are actions that must execute during the rendering of the page initialization script
        /// </summary>
        public Action<IRenderContext>[] InitializationWriters
        {
            get { return _initializationWriters; }
            set
            {
                _initializationWriters = value;
                EnsurePageArea(PageArea.Initialization, value == null);
            }
        }

        private void EnsurePageArea(PageArea pageArea, bool delete)
        {
            if (delete)
                PageAreas = PageAreas.Where(a => a != pageArea).ToArray();
            else if (!PageAreas.Contains(pageArea))
                PageAreas = PageAreas.Concat(Enumerable.Repeat(pageArea, 1)).ToArray();
        }

        /// <summary>
        /// These CSS rules will be written to the style assets for the component
        /// </summary>
        public Action<ICssWriter>[] CssRules 
        {
            get { return _assetDeploymentMixin.CssRules; }
            set { _assetDeploymentMixin.CssRules = value; }
        }

        /// <summary>
        /// These JS functions will be written to the script assets for the component
        /// </summary>
        public Action<IJavascriptWriter>[] JavascriptFunctions
        {
            get { return _assetDeploymentMixin.JavascriptFunctions; }
            set { _assetDeploymentMixin.JavascriptFunctions = value; }
        }

        private AssetDeploymentMixin _assetDeploymentMixin;

        public Component(IComponentDependenciesFactory dependencies)
            : base(dependencies.DataConsumerFactory)
        {
            // DO NOT change the method signature of this constructor as
            // this would break all components in all applications that use
            // this framework!!

            Dependencies = dependencies;

            _assetDeploymentMixin = new AssetDeploymentMixin(
                this, 
                dependencies.CssWriterFactory,
                dependencies.JavascriptWriterFactory,
                GetCommentName);
        }

        protected void CopyTo(Component to)
        {
            base.CopyTo(to);

            if (_headWriters != null)
                to.HeadWriters = _headWriters;

            if (_scriptWriters != null)
                to.ScriptWriters = _scriptWriters;

            if (_styleWriters != null)
                to.StyleWriters = _styleWriters;

            if (_bodyWriters != null)
                to.BodyWriters = _bodyWriters;

            if (_initializationWriters != null)
                to.InitializationWriters = _initializationWriters;

            if (CssRules != null)
                to.CssRules = CssRules;

            if (JavascriptFunctions != null)
                to.JavascriptFunctions = JavascriptFunctions;
        }

        public virtual object Clone()
        {
            var constructor = GetType().GetConstructor(new[] { typeof(IComponentDependenciesFactory) });
            if (constructor == null)
                throw new Exception("Unable to clone component " + GetType().FullName);

            var clone = constructor.Invoke(new object[] { Dependencies }) as Component;
            if (clone == null)
                throw new Exception("Cloning failed for " + GetType().FullName);

            CopyTo(clone);

            return clone;
        }

        protected override T PopulateDebugInfo<T>(DebugInfo debugInfo, int parentDepth, int childDepth)
        {
            var debugComponent = debugInfo as DebugComponent ?? new DebugComponent();

            return base.PopulateDebugInfo<T>(debugComponent, parentDepth, childDepth);
        }

        public override IEnumerable<PageArea> GetPageAreas()
        {
            return _assetDeploymentMixin.GetPageAreas(base.GetPageAreas());
        }

        protected virtual string GetCommentName()
        {
            return 
                (string.IsNullOrEmpty(Name) ? "unnamed component" : "'" + Name + "' component") +
                (Package == null ? string.Empty : " from the '" + Package.Name + "' package");
        }

        public virtual IWriteResult WritePageArea(
            IRenderContext context, 
            PageArea pageArea)
        {
            if (pageArea == PageArea.Head)
            {
                if (_headWriters != null && _headWriters.Length > 0)
                {
                    if (context.IncludeComments)
                        context.Html.WriteComment(GetCommentName());

                    for (var i = 0; i < _headWriters.Length; i++)
                        _headWriters[i](context);
                }
            }

            if (pageArea == PageArea.Scripts)
            {
                if (_scriptWriters != null && _scriptWriters.Length > 0)
                {
                    if (context.IncludeComments)
                        context.Html.WriteComment(GetCommentName());

                    for (var i = 0; i < _scriptWriters.Length; i++)
                        _scriptWriters[i](context);
                }
            }

            if (pageArea == PageArea.Styles)
            {
                if (_styleWriters != null && _styleWriters.Length > 0)
                {
                    if (context.IncludeComments)
                        context.Html.WriteComment(GetCommentName());

                    for (var i = 0; i < _styleWriters.Length; i++)
                        _styleWriters[i](context);
                }
            }

            if (pageArea == PageArea.Body)
            {
                if (_bodyWriters != null && _bodyWriters.Length > 0)
                {
                    if (context.IncludeComments)
                        context.Html.WriteComment(GetCommentName());

                    for (var i = 0; i < _bodyWriters.Length; i++)
                        _bodyWriters[i](context);
                }
            }

            if (pageArea == PageArea.Initialization)
            {
                if (_initializationWriters != null && _initializationWriters.Length > 0)
                {
                    if (context.IncludeComments)
                        context.Html.WriteComment(GetCommentName());

                    for (var i = 0; i < _initializationWriters.Length; i++)
                        _initializationWriters[i](context);
                }
            }

            _assetDeploymentMixin.WritePageArea(context, pageArea);

            return WriteResult.Continue();
        }

        public override IWriteResult WriteStaticCss(ICssWriter writer)
        {
            return _assetDeploymentMixin.WriteStaticCss(writer);
        }

        public override IWriteResult WriteStaticJavascript(IJavascriptWriter writer)
        {
            return _assetDeploymentMixin.WriteStaticJavascript(writer);
        }
    }
}
