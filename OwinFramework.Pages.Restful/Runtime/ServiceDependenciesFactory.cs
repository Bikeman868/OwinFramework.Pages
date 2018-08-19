using System;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Restful.Runtime
{
    internal class ServiceDependenciesFactory : IServiceDependenciesFactory
    {
        private readonly IRenderContextFactory _renderContextFactory;

        public IAssetManager AssetManager { get; private set; }
        public INameManager NameManager { get; private set; }
        public IDataConsumerFactory DataConsumerFactory { get; private set; }

        public ServiceDependenciesFactory(
            IRenderContextFactory renderContextFactory,
            IAssetManager assetManager,
            INameManager nameManager)
        {
            _renderContextFactory = renderContextFactory;
            AssetManager = assetManager;
            NameManager = nameManager;
        }

        public IServiceDependencies Create(IOwinContext context, Action<IOwinContext, Func<string>> trace)
        {
            return new ServiceDependencies(
                _renderContextFactory.Create(trace), 
                AssetManager, 
                NameManager)
                .Initialize(context);
        }
    }
}
