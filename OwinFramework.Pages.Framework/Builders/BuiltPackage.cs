using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;

namespace OwinFramework.Pages.Framework.Builders
{
    internal class BuiltPackage : IPackage
    {
        public string Name { get; set; }
        public string NamespaceName { get; set; }
        public IModule Module { get; set; }

        public IPackage Build(IFluentBuilder builder)
        {
            // Packages defined declaratively do not build anything within them
            return this;
        }
    }
}
