using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.Pages.Core.Interfaces.DataModel
{
    /// <summary>
    /// Factory for data dependencies
    /// </summary>
    public interface IDataDependencyFactory
    {
        /// <summary>
        /// Creates and initial;izes a new data dependency
        /// </summary>
        IDataDependency Create(Type type, string scopeName = null);
    }
}
