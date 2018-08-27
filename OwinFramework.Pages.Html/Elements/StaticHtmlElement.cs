using System;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Elements
{
    /// <summary>
    /// This element is used internally within the framework to write small
    /// amounts of html, for example containers around the regions of a layout.
    /// </summary>
    public class StaticHtmlElement : Element
    {
        public Action<IHtmlWriter> WriteAction;
        public string Comment;
        public override ElementType ElementType { get { return ElementType.Unnamed; } }

        public StaticHtmlElement() : base(null) { }

        public IWriteResult WriteHtml(IRenderContext context)
        {
            if (context.IncludeComments && !string.IsNullOrEmpty(Comment))
                context.Html.WriteComment(Comment);

            WriteAction(context.Html);
            return WriteResult.Continue();
        }

        protected override DebugInfo PopulateDebugInfo(DebugInfo debugInfo)
        {
            debugInfo.Type = "Static HTML";
            return base.PopulateDebugInfo(debugInfo);
        }
    }
}
