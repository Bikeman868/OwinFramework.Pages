using System;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;

namespace OwinFramework.Pages.Framework.Builders
{
    /// <summary>
    /// Plug-in to the fluent builder for building packages.
    /// Uses the supplied Package or constructs a new Package instance.
    /// Returns a fluent interface for defining the package characteristics
    /// </summary>
    internal class PackageBuilder : IPackageBuilder
    {
        private readonly IFluentBuilder _fluentBuilder;
        private readonly IPackageDependenciesFactory _packageDependenciesFactory;
        private readonly IElementConfiguror _elementConfiguror;
        private readonly INameManager _nameManager;

        public PackageBuilder(
            IPackageDependenciesFactory packageDependenciesFactory,
            IElementConfiguror elementConfiguror,
            INameManager nameManager,
            IFluentBuilder fluentBuilder)
        {
            _packageDependenciesFactory = packageDependenciesFactory;
            _elementConfiguror = elementConfiguror;
            _nameManager = nameManager;
            _fluentBuilder = fluentBuilder;
        }

        IPackageDefinition IPackageBuilder.BuildUpPackage(object packageInstance, Type declaringType)
        {
            var package = packageInstance as Core.Interfaces.IPackage ?? new Runtime.Package(_packageDependenciesFactory);
            if (declaringType == null) declaringType = (packageInstance ?? package).GetType();

            var packageDefinition = new PackageDefinition(package, _fluentBuilder, _nameManager);

            var attributes = new AttributeSet(declaringType);
            _elementConfiguror.Configure(packageDefinition, attributes);

            return packageDefinition;
        }
    }
}