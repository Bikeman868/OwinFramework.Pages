using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Elements
{
    internal class BuiltModule : Module
    {
        public BuiltModule(IModuleDependenciesFactory dependencies)
            : base(dependencies)
        { }
    }
}
