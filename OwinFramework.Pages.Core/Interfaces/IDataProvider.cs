using OwinFramework.Pages.Core.Debug;

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
    public interface IDataProvider
    {
        /// <summary>
        /// Gets and sets the name of this data provider. This allows other elements to
        /// reference this provider by name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets and sets the package. Setting a package puts the name of the data provider
        /// into a namespace
        /// </summary>
        IPackage Package { get; set; }

        /// <summary>
        /// Gets debugging information from this data provider
        /// </summary>
        DebugDataProvider GetDebugInfo();
    }
}
