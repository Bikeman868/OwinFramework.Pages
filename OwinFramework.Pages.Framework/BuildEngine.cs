using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Framework.Builders;

namespace OwinFramework.Pages.Framework
{
    /// <summary>
    /// This build engine provides builders for Packages and Data Providers
    /// </summary>
    public class BuildEngine: IBuildEngine
    {
        private readonly IPackageDependenciesFactory _packageDependenciesFactory;
        private readonly IElementConfiguror _elementConfiguror;
        private readonly IDataProviderDependenciesFactory _dataProviderDependenciesFactory;
        private readonly INameManager _nameManager;

        public BuildEngine(
            IPackageDependenciesFactory packageDependenciesFactory,
            IElementConfiguror elementConfiguror, 
            INameManager nameManager, 
            IDataProviderDependenciesFactory dataProviderDependenciesFactory)
        {
            _packageDependenciesFactory = packageDependenciesFactory;
            _elementConfiguror = elementConfiguror;
            _nameManager = nameManager;
            _dataProviderDependenciesFactory = dataProviderDependenciesFactory;
        }

        public void Install(IFluentBuilder builder)
        {
            builder.PackageBuilder = new PackageBuilder(
                _packageDependenciesFactory,
                _elementConfiguror,
                _nameManager,
                builder);

            builder.DataProviderBuilder = new DataProviderBuilder(
                _dataProviderDependenciesFactory,
                _elementConfiguror,
                _nameManager,
                builder);
        }
    }
}
