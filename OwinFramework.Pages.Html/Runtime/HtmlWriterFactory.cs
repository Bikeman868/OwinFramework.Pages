using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class HtmlWriterFactory: IHtmlWriterFactory
    {
        private readonly IStringBuilderFactory _stringBuilderFactory;

        public HtmlWriterFactory(
            IStringBuilderFactory stringBuilderFactory)
        {
            _stringBuilderFactory = stringBuilderFactory;
        }

        IHtmlWriter IHtmlWriterFactory.Create(HtmlFormat format, bool indented, bool includeComments)
        {
            return new HtmlWriter(_stringBuilderFactory) 
            { 
                Format = format,
                Indented = indented,
                IncludeComments = includeComments
            };
        }
    }
}
