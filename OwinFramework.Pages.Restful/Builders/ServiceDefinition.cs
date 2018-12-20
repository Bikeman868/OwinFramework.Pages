using System;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Restful.Runtime;

namespace OwinFramework.Pages.Restful.Builders
{
    internal class ServiceDefinition : IServiceDefinition
    {
        private readonly INameManager _nameManager;
        private readonly Service _service;

        public ServiceDefinition(
            Service service,
            INameManager nameManager,
            IPackage package,
            Type declaringType)
        {
            _nameManager = nameManager;
            _service = service;

            if (package != null)
                _service.Package = package;

            _service.DeclaringType = declaringType;
        }

        public IServiceDefinition Name(string name)
        {
            _service.Name = name;
            return this;
        }

        public IServiceDefinition PartOf(IPackage package)
        {
            _service.Package = package;
            return this;
        }

        public IServiceDefinition PartOf(string packageName)
        {
            if (string.IsNullOrEmpty(packageName)) return this;

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolvePackageNames,
                (nm, s, n) => s.Package = nm.ResolvePackage(n),
                _service,
                packageName);

            return this;
        }

        public IServiceDefinition DeployIn(IModule module)
        {
            _service.Module = module;
            return this;
        }

        public IServiceDefinition DeployIn(string moduleName)
        {
            if (string.IsNullOrEmpty(moduleName)) return this;

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                (nm, s, n) => s.Module = nm.ResolveModule(n),
                _service,
                moduleName);

            return this;
        }

        public IServiceDefinition RequiredPermission(string requiredPermission, bool endpointSpecificPermission)
        {
            _service.RequiredPermission = requiredPermission;
            _service.EndpointSpecificPermission = endpointSpecificPermission;
            return this;
        }

        public IServiceDefinition Serialization(Type requestDeserializer, Type responseSerializer)
        {
            return this;
        }

        public IServiceDefinition Cache(string category, CachePriority priority)
        {
            _service.CacheCategory = category;
            _service.CachePriority = priority;
            return this;
        }

        public IServiceDefinition Route(string basePath, Method[] methods, int priority)
        {
            if (string.IsNullOrEmpty(basePath))
            {
                basePath = "/";
            }
            else
            {
                if (!basePath.EndsWith("/"))
                    basePath += "/";

                if (!basePath.StartsWith("/"))
                    basePath = "/" + basePath;
            }

            _service.BasePath = basePath;
            _service.Methods = methods;
            _service.RoutingPriority = priority;

            return this;
        }

        public IService Build()
        {
            _nameManager.AddResolutionHandler(NameResolutionPhase.InitializeRunables, () => _service.Initialize());
            return _service;
        }
    }
}
