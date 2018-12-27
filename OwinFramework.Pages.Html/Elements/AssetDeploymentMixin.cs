using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.Pages.Html.Elements
{
    internal class AssetDeploymentMixin
    {
        public Action<ICssWriter>[] CssRules;
        public Action<IJavascriptWriter>[] JavascriptFunctions;

        private Element _element;
        private readonly ICssWriterFactory _cssWriterFactory;
        private readonly IJavascriptWriterFactory _javascriptWriterFactory;
        private readonly Func<string> _commentNameFunc;

        public AssetDeploymentMixin(
            Element element,
            ICssWriterFactory cssWriterFactory,
            IJavascriptWriterFactory javascriptWriterFactory,
            Func<string> commentNameFunc)
        {
            _element = element;
            _cssWriterFactory = cssWriterFactory;
            _javascriptWriterFactory = javascriptWriterFactory;
            _commentNameFunc = commentNameFunc;
        }

        public IEnumerable<PageArea> GetPageAreas(
            IEnumerable<PageArea> inheritedPageAreas)
        {
            if (_element.AssetDeployment != AssetDeployment.InPage)
                return inheritedPageAreas;

            var pageAreas = inheritedPageAreas.ToList();

            if (CssRules != null && CssRules.Length > 0)
                pageAreas.Add(PageArea.Styles);

            if (JavascriptFunctions != null && JavascriptFunctions.Length > 0)
                pageAreas.Add(PageArea.Scripts);

            return pageAreas;
        }

        public void WritePageArea(
            IRenderContext context,
            PageArea pageArea)
        {
            if (_element.AssetDeployment != AssetDeployment.InPage)
                return;

            if (pageArea == PageArea.Styles)
            {
                if (CssRules != null && CssRules.Length > 0)
                {
                    var writer = _cssWriterFactory.Create(context);

                    for (var i = 0; i < CssRules.Length; i++)
                        CssRules[i](writer);

                    if (context.IncludeComments)
                        context.Html.WriteComment("css rules for " + _commentNameFunc());

                    context.Html.WriteOpenTag("style");
                    context.Html.WriteLine();
                    writer.ToHtml(context.Html);
                    context.Html.WriteCloseTag("style");
                    context.Html.WriteLine();
                }
            }
            else if (pageArea == PageArea.Scripts)
            {
                if (JavascriptFunctions != null && JavascriptFunctions.Length > 0)
                {
                    var writer = _javascriptWriterFactory.Create(context);

                    for (var i = 0; i < JavascriptFunctions.Length; i++)
                        JavascriptFunctions[i](writer);

                    if (context.IncludeComments)
                        context.Html.WriteComment("javascript functions for " + _commentNameFunc());

                    context.Html.WriteScriptOpen();
                    writer.ToHtml(context.Html);
                    context.Html.WriteScriptClose();
                }
            }
        }

        public IWriteResult WriteStaticCss(ICssWriter writer)
        {
            if (CssRules != null && CssRules.Length > 0)
            {
                if (writer.IncludeComments)
                    writer.WriteComment("css rules for " + _commentNameFunc());

                for (var i = 0; i < CssRules.Length; i++)
                    CssRules[i](writer);
            }

            return WriteResult.Continue();
        }

        public IWriteResult WriteStaticJavascript(IJavascriptWriter writer)
        {
            if (JavascriptFunctions != null && JavascriptFunctions.Length > 0)
            {
                if (writer.IncludeComments)
                {
                    writer.WriteComment(
                        "javascript functions for " + _commentNameFunc(),
                        CommentStyle.SingleLineC,
                        _element.Package);
                }

                for (var i = 0; i < JavascriptFunctions.Length; i++)
                    JavascriptFunctions[i](writer);
            }

            return WriteResult.Continue();
        }
    }
}
