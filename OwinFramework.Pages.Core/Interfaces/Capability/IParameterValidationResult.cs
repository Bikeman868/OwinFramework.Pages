using System;

namespace OwinFramework.Pages.Core.Interfaces.Capability
{
    /// <summary>
    /// Defines the structure that must be returned from 
    /// REST service parameter validation functions
    /// </summary>
    public interface IParameterValidationResult
    {
        /// <summary>
        /// Was the validation successful? Is the parameter valid?
        /// </summary>
        bool Success { get; set; }

        /// <summary>
        /// The value that was parsed fom the parameter
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// The type of the object in the 'Value' property
        /// </summary>
        Type Type { get; set; }

        /// <summary>
        /// When 'Success' is false this property must contain
        /// a description of why the parameter is invalid
        /// </summary>
        string ErrorMessage { get; set; }
    }
}
