using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
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
    public class Component : Element, IComponent
    {
        public override ElementType ElementType { get { return ElementType.Component; } }

        protected readonly IComponentDependenciesFactory Dependencies;

        public Action<IRenderContext>[] HtmlWriters;

        public Action<ICssWriter>[] CssRules 
        {
            get { return _assetDeploymentMixin.CssRules; }
            set { _assetDeploymentMixin.CssRules = value; }
        }

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
            if (pageArea == PageArea.Body)
            {
                if (context.IncludeComments)
                    context.Html.WriteComment(GetCommentName());

                if (HtmlWriters != null)
                {
                    for (var i = 0; i < HtmlWriters.Length; i++)
                        HtmlWriters[i](context);
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
