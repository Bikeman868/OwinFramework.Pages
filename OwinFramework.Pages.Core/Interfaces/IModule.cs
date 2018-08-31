using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces
{
    /// <summary>
    /// A module is a collection of pages that share the same CSS and JavaScript files
    /// </summary>
    public interface IModule: IDeployable, INamed
    {
    }
}
