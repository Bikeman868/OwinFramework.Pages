using System;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces
{
    /// <summary>
    /// A service responds to browser requests by executing business logic
    /// and returning data in response. The response data can be any type.
    /// </summary>
    public interface IService : INamed, IPackagable
    {
        /// <summary>
        /// This method is called after all names have been resolved and all
        /// elements have been wired together. This is where the service has an
        /// oportunity to do work once that makes the rendering of each instance
        /// faster.
        /// </summary>
        void Initialize(Func<Type, object> factory);
    }
}
