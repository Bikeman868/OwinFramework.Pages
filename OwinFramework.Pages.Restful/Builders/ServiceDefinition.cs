using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.Pages.Core.Interfaces.Builder;

namespace OwinFramework.Pages.Restful.Builders
{
    internal class ServiceDefinition : IServiceDefinition
    {
        public IServiceDefinition Name(string name)
        {
            throw new NotImplementedException();
        }

        public IServiceDefinition PartOf(Core.Interfaces.IPackage package)
        {
            throw new NotImplementedException();
        }

        public IServiceDefinition PartOf(string packageName)
        {
            throw new NotImplementedException();
        }

        public IServiceDefinition DeployIn(Core.Interfaces.IModule module)
        {
            throw new NotImplementedException();
        }

        public IServiceDefinition DeployIn(string moduleName)
        {
            throw new NotImplementedException();
        }

        public IServiceDefinition AssetDeployment(Core.Enums.AssetDeployment assetDeployment)
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

        public Core.Interfaces.IService Build()
        {
            throw new NotImplementedException();
        }

        public IPageDefinition Route(string path, int priority, params Core.Enums.Methods[] methods)
        {
            throw new NotImplementedException();
        }

        public IPageDefinition Route(Core.Interfaces.Runtime.IRequestFilter filter, int priority = 0)
        {
            throw new NotImplementedException();
        }

        public IServiceDefinition BindTo<T>(string scope = null) where T : class
        {
            throw new NotImplementedException();
        }

        public IServiceDefinition BindTo(Type dataType, string scope = null)
        {
            throw new NotImplementedException();
        }

        public IServiceDefinition DataScope(Type dataType, string scopeName)
        {
            throw new NotImplementedException();
        }

        public IPageDefinition DataProvider(Core.Interfaces.IDataProvider dataProvider)
        {
            throw new NotImplementedException();
        }
    }
}
