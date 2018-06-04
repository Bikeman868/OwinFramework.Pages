using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces
{
    /// <summary>
    /// A service responds to browser requests by executing business logic
    /// and returning data in response. The response data can be any mime type.
    /// </summary>
    public interface IService : IElement, IRunable
    {
    }
}
