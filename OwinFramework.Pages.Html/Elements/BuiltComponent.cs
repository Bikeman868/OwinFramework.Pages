﻿using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Elements
{
    internal class BuiltComponent : Component
    {
        public List<Action<IRenderContext>> HtmlWriters;
        public List<Action<ICssWriter>> CssRules;
        public List<Action<IJavascriptWriter>> JavascriptFunctions;

        public BuiltComponent(IComponentDependenciesFactory dependencies)
            : base(dependencies)
        { }

        public override IWriteResult WriteHtml(IRenderContext context, bool includeChildren)
        {
            if (context.IncludeComments)
                context.Html.WriteComment(
                    (string.IsNullOrEmpty(Name) ? "unnamed" : Name) +
                    (Package == null ? " component" : " component from the " + Package.Name + " package"));

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
                writer.WriteComment(
                    "css rules for " +
                    (string.IsNullOrEmpty(Name) ? "unnamed" : Name) +
                    (Package == null ? " component" : " component from the " + Package.Name + " package"));

                foreach (var rule in CssRules)
                    rule(writer);
            }

            return WriteResult.Continue();
        }

        public override IWriteResult WriteStaticJavascript(IJavascriptWriter writer)
        {
            if (JavascriptFunctions != null && JavascriptFunctions.Count > 0)
            {
                writer.WriteComment(
                    "javascript functions for " +
                    (string.IsNullOrEmpty(Name) ? "unnamed" : Name) +
                    (Package == null ? " component" : " component from the " + Package.Name + " package"),
                    CommentStyle.SingleLineC,
                    Package);

                foreach (var javascriptFunction in JavascriptFunctions)
                    javascriptFunction(writer);
            }

            return WriteResult.Continue();
        }
    }
}