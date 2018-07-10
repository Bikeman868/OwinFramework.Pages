using System;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Restful.Runtime;
using OwinFramework.Pages.Core.Attributes;

namespace OwinFramework.Pages.Restful.Builders
{
    internal class ServiceBuilder: IServiceBuilder
    {
        private readonly IServiceDependenciesFactory _serviceDependenciesFactory;
        private readonly IElementConfiguror _elementConfiguror;

        public ServiceBuilder(
            IServiceDependenciesFactory serviceDependenciesFactory,
            IElementConfiguror elementConfiguror)
        {
            _serviceDependenciesFactory = serviceDependenciesFactory;
            _elementConfiguror = elementConfiguror;
        }

        public IServiceDefinition BuildUpService(object serviceInstance = null, Type declaringType = null, IPackage package = null)
        {
            var service = serviceInstance as Service ?? new Service(_serviceDependenciesFactory);
            if (declaringType == null) declaringType = (serviceInstance ?? service).GetType();

            var attributes = new AttributeSet(declaringType);
            _elementConfiguror.Configure(service, attributes);

            var serviceDefinition = new ServiceDefinition();
            Configure(serviceDefinition, attributes);

            return serviceDefinition;
        }

        private void Configure(IServiceDefinition serviceDefinition, AttributeSet attributes)
        {
        }
    }
}
