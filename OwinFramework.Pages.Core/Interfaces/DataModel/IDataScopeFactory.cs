using System;
using System.Collections.Generic;

namespace OwinFramework.Pages.Core.Interfaces.DataModel
{
    /// <summary>
    /// A factory for data scopes
    /// </summary>
    public interface IDataScopeFactory
    {
        /// <summary>
        /// Creates and initializes a new data scope
        /// </summary>
        IDataScope Create(Type dataType, string scopeName = null);

        /// <summary>
        /// Creates and initializes a new data scope
        /// </summary>
        IDataScope Create(string scopeName, Type dataType = null);
    }
}
