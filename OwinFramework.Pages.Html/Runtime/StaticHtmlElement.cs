using System;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    public class StaticHtmlElement : Element
    {
        public Action<IHtmlWriter> WriteAction;
        public string Comment;
        public override ElementType ElementType { get { return ElementType.Unnamed; } }

        public override IWriteResult WriteHtml(
            IRenderContext renderContext,
            bool includeChildren)
        {
            if (renderContext.IncludeComments && !string.IsNullOrEmpty(Comment))
                renderContext.Html.WriteComment(Comment);

            WriteAction(renderContext.Html);
            return WriteResult.Continue();
        }
    }
}
