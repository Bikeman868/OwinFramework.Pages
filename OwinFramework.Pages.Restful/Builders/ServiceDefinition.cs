using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Restful.Runtime;

namespace OwinFramework.Pages.Restful.Builders
{
    internal class ServiceDefinition : IServiceDefinition
    {
        private readonly IRequestRouter _requestRouter;
        private readonly INameManager _nameManager;
        private readonly IFluentBuilder _fluentBuilder;
        private readonly Type _declaringType;
        private readonly Service _service;

        public ServiceDefinition(
            Service service,
            IRequestRouter requestRouter,
            INameManager nameManager,
            IFluentBuilder fluentBuilder,
            IPackage package,
            Type declaringType)
        {
            _requestRouter = requestRouter;
            _nameManager = nameManager;
            _fluentBuilder = fluentBuilder;
            _declaringType = declaringType;
            _service = service;

            if (package != null)
                _service.Package = package;
        }

        public IServiceDefinition Name(string name)
        {
            throw new NotImplementedException();
        }

        public IServiceDefinition PartOf(IPackage package)
        {
            throw new NotImplementedException();
        }

        public IServiceDefinition PartOf(string packageName)
        {
            throw new NotImplementedException();
        }

        public IServiceDefinition DeployIn(IModule module)
        {
            throw new NotImplementedException();
        }

        public IServiceDefinition DeployIn(string moduleName)
        {
            throw new NotImplementedException();
        }

        public IServiceDefinition AssetDeployment(AssetDeployment assetDeployment)
        {
            throw new NotImplementedException();
        }

        public IServiceDefinition BindTo<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public IServiceDefinition BindTo(Type dataType)
        {
            throw new NotImplementedException();
        }

        public IServiceDefinition DataScope(string scopeName)
        {
            throw new NotImplementedException();
        }

        public IServiceDefinition DataProvider(string providerName)
        {
            throw new NotImplementedException();
        }

        public IPageDefinition Route(string path, int priority, params Methods[] methods)
        {
            throw new NotImplementedException();
        }

        public IPageDefinition Route(IRequestFilter filter, int priority = 0)
        {
            throw new NotImplementedException();
        }

        public IServiceDefinition BindTo<T>(string scope) where T : class
        {
            throw new NotImplementedException();
        }

        public IServiceDefinition BindTo(Type dataType, string scope)
        {
            throw new NotImplementedException();
        }

        public IServiceDefinition DataScope(Type dataType, string scopeName)
        {
            throw new NotImplementedException();
        }

        public IPageDefinition DataProvider(IDataProvider dataProvider)
        {
            throw new NotImplementedException();
        }

        public IService Build()
        {
            _nameManager.AddResolutionHandler(NameResolutionPhase.InitializeRunables, () => _service.Initialize());
            return _service;
        }

    }
}
