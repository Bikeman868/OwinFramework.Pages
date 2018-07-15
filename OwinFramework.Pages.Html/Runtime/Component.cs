using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Elements;

namespace OwinFramework.Pages.Html.Runtime
{
    /// <summary>
    /// Base implementation of IComponent. Inheriting from this olass will insulate you
    /// from any future additions to the IComponent interface
    /// </summary>
    public class Component : Element, IComponent
    {
        public override ElementType ElementType { get { return ElementType.Component; } }

        public List<Action<IRenderContext>> HtmlWriters;
        public List<Action<ICssWriter>> CssRules;
        public List<Action<IJavascriptWriter>> JavascriptFunctions;

        public Component(IComponentDependenciesFactory dependencies)
            : base(dependencies.DataConsumerFactory)
        {
            // DO NOT change the method signature of this constructor as
            // this would break all components in all applications that use
            // this framework!!
        }

        protected override DebugInfo PopulateDebugInfo(DebugInfo debugInfo)
        {
            var debugComponent = debugInfo as DebugComponent ?? new DebugComponent();

            return base.PopulateDebugInfo(debugComponent);
        }

        private string GetCommentName()
        {
            return 
                (string.IsNullOrEmpty(Name) ? "unnamed component" : "'" + Name + "' component") +
                (Package == null ? string.Empty : " from the '" + Package.Name + "' package");
        }

        public override IWriteResult WriteHtml(IRenderContext context, bool includeChildren)
        {
            if (context.IncludeComments)
                context.Html.WriteComment(GetCommentName());

            if (HtmlWriters != null)
            {
                foreach (var writer in HtmlWriters)
                    writer(context);
            }

            return WriteResult.Continue();
        }

        public override IWriteResult WriteStaticCss(ICssWriter writer)
        {
            if (CssRules != null && CssRules.Count > 0)
            {
                if (writer.IncludeComments)
                    writer.WriteComment("css rules for " + GetCommentName());

                foreach (var rule in CssRules)
                    rule(writer);
            }

            return WriteResult.Continue();
        }

        public override IWriteResult WriteStaticJavascript(IJavascriptWriter writer)
        {
            if (JavascriptFunctions != null && JavascriptFunctions.Count > 0)
            {
                if (writer.IncludeComments)
                {
                    writer.WriteComment(
                        "javascript functions for " + GetCommentName(),
                        CommentStyle.SingleLineC,
                        Package);
                }

                foreach (var javascriptFunction in JavascriptFunctions)
                    javascriptFunction(writer);
            }

            return WriteResult.Continue();
        }
    }
}
