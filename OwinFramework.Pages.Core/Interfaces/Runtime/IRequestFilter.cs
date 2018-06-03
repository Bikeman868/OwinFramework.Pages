using Microsoft.Owin;

namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// Defines a function that tests the incomming request to see
    /// if it matches some filtering condition
    /// </summary>
    public interface IRequestFilter
    {
        /// <summary>
        /// Returns true if the request atches this filter
        /// </summary>
        bool IsMatch(IOwinContext context);

        /// <summary>
        /// Returns a short description of the requests that match - output
        /// into self documentation
        /// </summary>
        string Description { get; }
    }
}
