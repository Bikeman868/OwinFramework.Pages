using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Interfaces;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class CssWriterFactory : ICssWriterFactory
    {
        private readonly IFrameworkConfiguration _frameworkConfiguration;

        public CssWriterFactory(
            IFrameworkConfiguration htmlConfiguration)
        {
            _frameworkConfiguration = htmlConfiguration;
        }

        public ICssWriter Create()
        {
            return new CssWriter(_frameworkConfiguration);
        }

        public ICssWriter Create(IRenderContext context)
        {
            return new CssWriter(_frameworkConfiguration)
            {
                Indented = context.Html.Indented,
                IncludeComments = context.IncludeComments
            };
        }
    }
}
