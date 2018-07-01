﻿using System;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    public class StaticHtmlElement : Element
    {
        public Action<IHtmlWriter> WriteAction;
        public string Comment;
        public override ElementType ElementType { get { return ElementType.Unnamed; } }

        public StaticHtmlElement() : base(null) { }

        public override IWriteResult WriteHtml(
            IRenderContext context,
            bool includeChildren)
        {
            if (context.IncludeComments && !string.IsNullOrEmpty(Comment))
                context.Html.WriteComment(Comment);

            WriteAction(context.Html);
            return WriteResult.Continue();
        }
    }
}
