using System;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Use the package builder to construct packages using a fluent syntax
    /// </summary>
    public interface IPackageBuilder
    {
        /// <summary>
        /// Starts building a new package or configuring an existing package
        /// </summary>
        /// <param name="packageInstance">Pass an instance that derives from Package
        /// to configure it directly, or pass any other instance type or null to 
        /// construct an instance of the Package class</param>
        /// <param name="declaringType">Type type to extract custom attributes from
        /// that can also define the behaviour of the package</param>
        /// <param name="factory">If the package includes services then it needs a 
        /// factory to build the services</param>
        IPackageDefinition BuildUpPackage(object packageInstance = null, Type declaringType = null, Func<Type, object> factory = null);
    }
}
