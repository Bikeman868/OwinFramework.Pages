using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.Pages.Core.Debug;

namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// Classes that implement this interface can be queried for information
    /// to help debug issues with the application
    /// </summary>
    public interface IDebuggable
    {
        /// <summary>
        /// Gets debuging information for this element
        /// </summary>
        DebugInfo GetDebugInfo();
    }
}
