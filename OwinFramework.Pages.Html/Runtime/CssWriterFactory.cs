using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class CssWriterFactory : ICssWriterFactory
    {
        public ICssWriter Create(HtmlFormat format, bool indented, bool includeComments)
        {
            return new CssWriter
            {
                Indented = indented,
                IncludeComments = includeComments
            };
        }
    }
}
