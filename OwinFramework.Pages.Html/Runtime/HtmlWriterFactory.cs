using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class HtmlWriterFactory: IHtmlWriterFactory
    {
        private readonly IStringBuilderFactory _stringBuilderFactory;
        private readonly IFrameworkConfiguration _frameworkConfiguration;

        public HtmlWriterFactory(
            IStringBuilderFactory stringBuilderFactory,
            IFrameworkConfiguration frameworkConfiguration)
        {
            _stringBuilderFactory = stringBuilderFactory;
            _frameworkConfiguration = frameworkConfiguration;
        }

        IHtmlWriter IHtmlWriterFactory.Create(IFrameworkConfiguration frameworkConfiguration)
        {
            return new HtmlWriter(_stringBuilderFactory, _frameworkConfiguration);
        }
    }
}
