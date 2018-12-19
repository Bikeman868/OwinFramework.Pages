using System;

namespace OwinFramework.Pages.Core.Interfaces.Capability
{
    /// <summary>
    /// Defines a class that can parse and validate parameters to service endpoints
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

        /// <summary>
        /// Return true if this parameter must be supplied
        /// </summary>
        bool IsRequired { get; }

        /// <summary>
        /// The data type that you must use to retrieve this parameter
        /// </summary>
        Type ParameterType { get; }
    }
}
