using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class JavascriptWriterFactory : IJavascriptWriterFactory
    {
        private IFrameworkConfiguration _frameworkConfiguration;

        public JavascriptWriterFactory(IFrameworkConfiguration frameworkConfiguration)
        {
            frameworkConfiguration.Subscribe(fc => _frameworkConfiguration = fc);
        }

        public IJavascriptWriter Create(IFrameworkConfiguration frameworkConfiguration)
        {
            return new JavascriptWriter
            {
                FrameworkConfiguration = frameworkConfiguration
            };
        }

        public IJavascriptWriter Create(IRenderContext context)
        {
            return new JavascriptWriter
            {
                FrameworkConfiguration = _frameworkConfiguration,
                IndentLevel = context.Html.IndentLevel
            };
        }
    }
}
