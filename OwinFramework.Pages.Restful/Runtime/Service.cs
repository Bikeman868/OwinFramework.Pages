using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Restful.Runtime
{
    /// <summary>
    /// Base implementation of IComponent. Inheriting from this olass will insulate you
    /// from any future additions to the IComponent interface
    /// </summary>
    public class Service : IService
    {
        public string Name { get; set; }
        public IPackage Package { get; set; }
        public string RequiredPermission { get; set; }
        public bool AllowAnonymous { get; set; }
        public Func<IOwinContext, bool> AuthenticationFunc { get { return null; } }

        public Service(IServiceDependenciesFactory serviceDependenciesFactory)
        {
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public Task Run(IOwinContext context, Action<IOwinContext, Func<string>> trace)
        {
            throw new NotImplementedException();
        }

        DebugInfo IRunable.GetDebugInfo()
        {
            throw new NotImplementedException();
        }

        public DebugService GetDebugInfo()
        {
            throw new NotImplementedException();
        }
    }
}
