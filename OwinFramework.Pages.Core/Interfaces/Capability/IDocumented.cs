using System.Collections.Generic;
using OwinFramework.InterfacesV1.Capability;

namespace OwinFramework.Pages.Core.Interfaces.Capability
{
    /// <summary>
    /// Elements can optionally implement this interface to provide documentation
    /// </summary>
    public interface IDocumented
    {
        /// <summary>
        /// A description of this page or service
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// An Html fragment to include in the documentation with examples of
        /// how to call this page or service
        /// </summary>
        string Examples { get; }

        /// <summary>
        /// A list of supported methods, query string parameters, custom headers etc
        /// </summary>
        IList<IEndpointAttributeDocumentation> Attributes { get; }
    }
}
