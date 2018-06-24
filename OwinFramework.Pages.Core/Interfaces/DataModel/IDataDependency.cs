using System;

namespace OwinFramework.Pages.Core.Interfaces.DataModel
{
    /// <summary>
    /// Elements can have a collection of these to indicate
    /// the data that they depend on. These are gathered up in
    /// the matching data scope providers to trigged the creation
    /// of a data context at runtime.
    /// </summary>
    public interface IDataDependency
    {
        /// <summary>
        /// The type of data that the element needs
        /// </summary>
        Type DataType { get; set; }

        /// <summary>
        /// Optional scope name. When specified the
        /// </summary>
        string ScopeName { get; set; }
    }
}
