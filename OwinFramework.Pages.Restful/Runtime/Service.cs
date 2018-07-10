using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;

namespace OwinFramework.Pages.Restful.Runtime
{
    /// <summary>
    /// Base implementation of IComponent. Inheriting from this olass will insulate you
    /// from any future additions to the IComponent interface
    /// </summary>
    public class Service : IService
    {
        public Service(IServiceDependenciesFactory serviceDependenciesFactory)
        {

        }

        public DebugService GetDebugInfo()
        {
            throw new System.NotImplementedException();
        }

        public void Initialize()
        {
            throw new System.NotImplementedException();
        }

        public string RequiredPermission
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        public bool AllowAnonymous
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        public System.Func<Microsoft.Owin.IOwinContext, bool> AuthenticationFunc
        {
            get { throw new System.NotImplementedException(); }
        }

        public System.Threading.Tasks.Task Run(Microsoft.Owin.IOwinContext context)
        {
            throw new System.NotImplementedException();
        }

        Core.Debug.DebugInfo Core.Interfaces.Runtime.IRunable.GetDebugInfo()
        {
            throw new System.NotImplementedException();
        }

        public string Name
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }

        public IPackage Package
        {
            get
            {
                throw new System.NotImplementedException();
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
