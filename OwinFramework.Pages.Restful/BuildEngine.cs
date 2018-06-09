using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Restful
{
    public class BuildEngine: IBuildEngine
    {
        private readonly IRequestRouter _requestRouter;
        private readonly INameManager _nameManager;

        public BuildEngine(
            IRequestRouter requestRouter,
            INameManager nameManager)
        {
            _requestRouter = requestRouter;
            _nameManager = nameManager;
        }

        public void Install(IFluentBuilder builder)
        {
            builder.ServiceBuilder = new ServiceBuilder();
        }
    }
}
