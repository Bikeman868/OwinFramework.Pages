using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Restful.Builders;

namespace OwinFramework.Pages.Restful
{
    /// <summary>
    /// This build engine provides builders for services
    /// </summary>
    public class BuildEngine: IBuildEngine
    {
        private readonly IServiceDependenciesFactory _serviceDependenciesFactory;
        private readonly IElementConfiguror _elementConfiguror;
        private readonly IRequestRouter _requestRouter;
        private readonly INameManager _nameManager;
        private readonly IFluentBuilder _fluentBuilder;

        public BuildEngine(
            IServiceDependenciesFactory serviceDependenciesFactory,
            IElementConfiguror elementConfiguror,
            IRequestRouter requestRouter,
            INameManager nameManager, 
            IFluentBuilder fluentBuilder)
        {
            _serviceDependenciesFactory = serviceDependenciesFactory;
            _elementConfiguror = elementConfiguror;
            _requestRouter = requestRouter;
            _nameManager = nameManager;
            _fluentBuilder = fluentBuilder;
        }

        public void Install(IFluentBuilder builder)
        {
            builder.ServiceBuilder = new ServiceBuilder(
                _serviceDependenciesFactory,
                _elementConfiguror,
                _fluentBuilder,
                _requestRouter,
                _nameManager);
        }
    }
}
