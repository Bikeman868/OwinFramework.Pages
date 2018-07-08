using System;

namespace OwinFramework.Pages.Core.Interfaces.DataModel
{
    /// <summary>
    /// A collection of these objects is attached to a scope provider
    /// to define the data scopes that it introduces for the elements
    /// beneath it.
    /// </summary>
    public interface IDataScope
    {
        /// <summary>
        /// The type of data whose scope is changed or null for all types
        /// </summary>
        Type DataType { get; set; }

        /// <summary>
        /// The name of the scope used to identify the data provider or
        /// null to override all scopes and resolve by type only
        /// </summary>
        string ScopeName { get; set; }

        /// <summary>
        /// Determines if this data scope can satisfy an element's data dependency
        /// </summary>
        bool IsMatch(IDataDependency dependency);
    }
}
