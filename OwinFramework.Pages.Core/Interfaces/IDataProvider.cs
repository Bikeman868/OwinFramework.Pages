using System;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces
{
    /// <summary>
    /// A data provider supplies and/or consumes data and has a name.
    /// Other elements can have a dependency of the data provider.
    /// The provider will always be executed prior to anything that
    /// depends on it.
    /// When you write a class that implements IDataProvider you would 
    /// typically implement IDataSupplier or IDataConsuler or both.
    /// </summary>
    public interface IDataProvider : IPackagable, INamed, IDataSupplier
    {
        /// <summary>
        /// Gets debugging information from this data provider
        /// </summary>
        new DebugDataProvider GetDebugInfo();

        /// <summary>
        /// Instructs the data provider to provide the specified type 
        /// in a given scope.
        /// </summary>
        void Add<T>(string scopeName = null);

        /// <summary>
        /// Instructs the data provider to provide the specified type 
        /// in a given scope.
        /// </summary>
        void Add(Type type, string scopeName = null);
    }
}
