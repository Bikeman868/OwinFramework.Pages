using System;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class JavascriptWriterFactory : IJavascriptWriterFactory
    {
        public IJavascriptWriter Create(HtmlFormat format, bool indented, bool includeComments)
        {
            return new JavascriptWriter
            {
                Indented = indented,
                IncludeComments = includeComments
            };
        }
    }
}
