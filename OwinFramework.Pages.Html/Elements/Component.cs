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
        public Action<ICssWriter>[] CssRules;
        public Action<IJavascriptWriter>[] JavascriptFunctions;

        public Component(IComponentDependenciesFactory dependencies)
            : base(dependencies.DataConsumerFactory)
        {
            // DO NOT change the method signature of this constructor as
            // this would break all components in all applications that use
            // this framework!!

            Dependencies = dependencies;
        }

        protected override T PopulateDebugInfo<T>(DebugInfo debugInfo, int parentDepth, int childDepth)
        {
            var debugComponent = debugInfo as DebugComponent ?? new DebugComponent();

            return base.PopulateDebugInfo<T>(debugComponent, parentDepth, childDepth);
        }

        public override IEnumerable<PageArea> GetPageAreas()
        {
            if (AssetDeployment != AssetDeployment.InPage)
                return base.GetPageAreas().ToList();

            var pageAreas = base.GetPageAreas().ToList();

            if (CssRules != null && CssRules.Length > 0)
                pageAreas.Add(PageArea.Styles);

            if (JavascriptFunctions != null && JavascriptFunctions.Length > 0)
                pageAreas.Add(PageArea.Scripts);

            return pageAreas;
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

            if (AssetDeployment == AssetDeployment.InPage)
            {
                if (pageArea == PageArea.Styles)
                {
                    if (CssRules != null && CssRules.Length > 0)
                    {
                        var writer = Dependencies.CssWriterFactory.Create(context);

                        for (var i = 0; i < CssRules.Length; i++)
                            CssRules[i](writer);

                        if (context.IncludeComments)
                            context.Html.WriteComment("css rules for " + GetCommentName());

                        context.Html.WriteOpenTag("style");
                        writer.ToHtml(context.Html);
                        context.Html.WriteCloseTag("style");
                    }
                }
                else if (pageArea == PageArea.Scripts)
                {
                    if (JavascriptFunctions != null && JavascriptFunctions.Length > 0)
                    {
                        var writer = Dependencies.JavascriptWriterFactory.Create(context);

                        for (var i = 0; i < JavascriptFunctions.Length; i++)
                            JavascriptFunctions[i](writer);

                        if (context.IncludeComments)
                            context.Html.WriteComment("javascript functions for " + GetCommentName());

                        context.Html.WriteScriptOpen();
                        writer.ToHtml(context.Html);
                        context.Html.WriteScriptClose();
                    }
                }
            }

            return WriteResult.Continue();
        }

        public override IWriteResult WriteStaticCss(ICssWriter writer)
        {
            if (CssRules != null && CssRules.Length > 0)
            {
                if (writer.IncludeComments)
                    writer.WriteComment("css rules for " + GetCommentName());

                for (var i = 0; i < CssRules.Length; i++)
                    CssRules[i](writer);
            }

            return WriteResult.Continue();
        }

        public override IWriteResult WriteStaticJavascript(IJavascriptWriter writer)
        {
            if (JavascriptFunctions != null && JavascriptFunctions.Length > 0)
            {
                if (writer.IncludeComments)
                {
                    writer.WriteComment(
                        "javascript functions for " + GetCommentName(),
                        CommentStyle.SingleLineC,
                        Package);
                }

                for (var i = 0; i < JavascriptFunctions.Length; i++)
                    JavascriptFunctions[i](writer);
            }

            return WriteResult.Continue();
        }
    }
}
