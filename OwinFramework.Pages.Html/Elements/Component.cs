using System;
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

        public Action<IRenderContext>[] HtmlWriters;
        public Action<ICssWriter>[] CssRules;
        public Action<IJavascriptWriter>[] JavascriptFunctions;

        public Component(IComponentDependenciesFactory dependencies)
            : base(dependencies.DataConsumerFactory)
        {
            // DO NOT change the method signature of this constructor as
            // this would break all components in all applications that use
            // this framework!!
        }

        protected override DebugInfo PopulateDebugInfo(DebugInfo debugInfo, int parentDepth, int childDepth)
        {
            var debugComponent = debugInfo as DebugComponent ?? new DebugComponent();

            return base.PopulateDebugInfo(debugComponent, parentDepth, childDepth);
        }

        private string GetCommentName()
        {
            return 
                (string.IsNullOrEmpty(Name) ? "unnamed component" : "'" + Name + "' component") +
                (Package == null ? string.Empty : " from the '" + Package.Name + "' package");
        }

        public virtual IWriteResult WritePageArea(
            IRenderContext context, 
            PageArea pageArea)
        {
            if (context.IncludeComments)
                context.Html.WriteComment(GetCommentName());

            if (HtmlWriters != null)
            {
                for (var i = 0; i < HtmlWriters.Length; i++)
                    HtmlWriters[i](context);
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
