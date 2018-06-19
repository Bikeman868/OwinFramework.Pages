using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;

namespace OwinFramework.Pages.Framework.Builders
{
    internal class BuiltPackage : IPackage
    {
        public string Name { get; set; }
        public string NamespaceName { get; set; }
        public IModule Module { get; set; }

        /// <summary>
        /// Builds a package
        /// </summary>
        /// <param name="fluentBuilder">A fluent builder that has a package context and
        /// builds everything within this package</param>
        public IPackage Build(IFluentBuilder fluentBuilder)
        {
            // Packages defined declaratively do not build anything within them
            return this;
        }
    }
}
