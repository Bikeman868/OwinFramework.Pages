using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.Pages.Core.Attributes;
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
        private readonly INameManager _nameManager;
        private readonly Func<Type, object> _factory;
        private readonly Service _service;

        private ClientScriptComponent _clientScriptComponent;

        public ServiceDefinition(
            Service service,
            INameManager nameManager,
            IPackage package,
            Type declaringType,
            Func<Type, object> factory)
        {
            _nameManager = nameManager;
            _factory = factory;
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
                (nm, s, n) =>
                {
                    s.Package = nm.ResolvePackage(n);
                    if (_clientScriptComponent != null)
                        _clientScriptComponent.Package = _service.Package;
                },
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
            _service.AllowAnonymous = false;
            _service.RequiredPermission = requiredPermission;
            _service.EndpointSpecificPermission = endpointSpecificPermission;
            return this;
        }

        public IServiceDefinition RequireIdentification()
        {
            _service.AllowAnonymous = false;
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

        public IServiceDefinition CreateComponent(string componentName)
        {
            _service.ClientScriptComponentName = componentName;
            return this;
        }

        public IService Build()
        {
            if (!string.IsNullOrEmpty(_service.ClientScriptComponentName) && !string.IsNullOrEmpty(_service.Name))
            {
                _clientScriptComponent = new ClientScriptComponent
                {
                    ServiceName = _service.Name,
                    Name = _service.ClientScriptComponentName,
                    Package = _service.Package,
                    AssetDeployment = AssetDeployment.PerModule,
                    Module = _service.Module
                };
                _nameManager.Register(_clientScriptComponent);
            }

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.CreateInstances,
                () =>
                {
                    _service.Initialize(_factory);

                    if (_clientScriptComponent != null)
                        _clientScriptComponent.ClientScript = _service.ClientScript;
                });
            return _service;
        }

        private class ClientScriptComponent: IComponent
        {
            public string ServiceName { get; set; }
            public string ClientScript { get; set; }
            public IModule Module { get; set; }
            public AssetDeployment AssetDeployment { get; set; }
            public IPackage Package{ get; set; }
            public ElementType ElementType{get { return ElementType.Component; }}
            public string Name { get; set; }

            public IWriteResult WritePageArea(IRenderContext context, PageArea pageArea)
            {
                return new WriteResult();
            }

            public IEnumerable<PageArea> GetPageAreas()
            {
                return Enumerable.Empty<PageArea>();
            }

            public IWriteResult WriteInPageStyles(ICssWriter writer, Func<ICssWriter, IWriteResult, IWriteResult> childrenWriter)
            {
                return new WriteResult();
            }

            public IWriteResult WriteInPageFunctions(IJavascriptWriter writer, Func<IJavascriptWriter, IWriteResult, IWriteResult> childrenWriter)
            {
                return new WriteResult();
            }

            public IWriteResult WriteStaticCss(ICssWriter writer)
            {
                return new WriteResult();
            }

            public IWriteResult WriteStaticJavascript(IJavascriptWriter writer)
            {
                writer.WriteClass(ServiceName + "Service", ClientScript, Package);
                return new WriteResult();
            }

            private class WriteResult: IWriteResult
            {
                bool IWriteResult.IsComplete
                {
                    get { return true; }
                }

                IWriteResult IWriteResult.Add(IWriteResult priorWriteResult)
                {
                    return this;
                }

                void IWriteResult.Wait(bool cancel)
                {
                }

                void IDisposable.Dispose()
                {
                }
            }
        }
    }
}
