using System;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Restful.Runtime;
using OwinFramework.Pages.Core.Attributes;

namespace OwinFramework.Pages.Restful.Builders
{
    internal class ServiceBuilder: IServiceBuilder
    {
        private readonly IRequestRouter _requestRouter;
        private readonly INameManager _nameManager;
        private readonly IServiceDependenciesFactory _serviceDependenciesFactory;
        private readonly IElementConfiguror _elementConfiguror;
        private readonly IFluentBuilder _fluentBuilder;

        public ServiceBuilder(
            IServiceDependenciesFactory serviceDependenciesFactory,
            IElementConfiguror elementConfiguror,
            IFluentBuilder fluentBuilder,
            IRequestRouter requestRouter,
            INameManager nameManager)
        {
            _serviceDependenciesFactory = serviceDependenciesFactory;
            _elementConfiguror = elementConfiguror;
            _fluentBuilder = fluentBuilder;
            _requestRouter = requestRouter;
            _nameManager = nameManager;
        }

        public IServiceDefinition BuildUpService(object serviceInstance = null, Type declaringType = null, IPackage package = null)
        {
            var service = serviceInstance as Service ?? new Service(_serviceDependenciesFactory);
            if (declaringType == null) declaringType = (serviceInstance ?? service).GetType();

            var serviceDefinition = new ServiceDefinition(service, _nameManager, package, declaringType);

            var attributes = new AttributeSet(declaringType);
            _elementConfiguror.Configure(serviceDefinition, attributes);

            return serviceDefinition;
        }
    }
}
