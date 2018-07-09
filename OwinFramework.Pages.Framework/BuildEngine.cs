using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Framework.Builders;

namespace OwinFramework.Pages.Framework
{
    public class BuildEngine: IBuildEngine
    {
        private readonly IPackageDependenciesFactory _packageDependenciesFactory;
        private readonly IElementConfiguror _elementConfiguror;

        public BuildEngine(
            IPackageDependenciesFactory packageDependenciesFactory,
            IElementConfiguror elementConfiguror)
        {
            _packageDependenciesFactory = packageDependenciesFactory;
            _elementConfiguror = elementConfiguror;
        }

        public void Install(IFluentBuilder builder)
        {
            builder.PackageBuilder = new PackageBuilder(
                _packageDependenciesFactory,
                _elementConfiguror,
                builder);
        }
    }
}
