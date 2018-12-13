using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.Pages.Restful.Interfaces
{
    /// <summary>
    /// Defines a class that can validate parameters to service endpoints
    /// </summary>
    public interface IParameterValidator
    {
        /// <summary>
        /// Returns a description of what is allowed in this parameter
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Checks the value of a service parameter and returns information
        /// about the validity of the parameter value
        /// </summary>
        IParameterValidationResult Check(string parameter);
    }
}
