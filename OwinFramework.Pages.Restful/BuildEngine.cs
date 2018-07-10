using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Restful.Builders;

namespace OwinFramework.Pages.Restful
{
    public class BuildEngine: IBuildEngine
    {
        private readonly IServiceDependenciesFactory _serviceDependenciesFactory;
        private readonly IElementConfiguror _elementConfiguror;
        private readonly IRequestRouter _requestRouter;
        private readonly INameManager _nameManager;

        public BuildEngine(
            IServiceDependenciesFactory serviceDependenciesFactory,
            IElementConfiguror elementConfiguror,
            IRequestRouter requestRouter,
            INameManager nameManager)
        {
            _serviceDependenciesFactory = serviceDependenciesFactory;
            _elementConfiguror = elementConfiguror;
            _requestRouter = requestRouter;
            _nameManager = nameManager;
        }

        public void Install(IFluentBuilder builder)
        {
            builder.ServiceBuilder = new ServiceBuilder(
                _serviceDependenciesFactory,
                _elementConfiguror);
        }
    }
}
