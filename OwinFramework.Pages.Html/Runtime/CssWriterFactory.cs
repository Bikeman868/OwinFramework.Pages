using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Interfaces;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class CssWriterFactory : ICssWriterFactory
    {
        private readonly IHtmlConfiguration _htmlConfiguration;

        public CssWriterFactory(
            IHtmlConfiguration htmlConfiguration)
        {
            _htmlConfiguration = htmlConfiguration;
        }

        public ICssWriter Create()
        {
            return new CssWriter(_htmlConfiguration);
        }

        public ICssWriter Create(IRenderContext context)
        {
            return new CssWriter(_htmlConfiguration)
            {
                Indented = context.Html.Indented,
                IncludeComments = context.IncludeComments
            };
        }
    }
}
