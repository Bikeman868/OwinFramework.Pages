using System;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Use the region builder to construct regions using a fluent syntax
    /// </summary>
    public interface IRegionBuilder
    {
        /// <summary>
        /// Starts building a new region or configuring an existing region
        /// </summary>
        /// <param name="regionInstance">Pass an instance of the Region class to 
        /// configure an instance of a derrived class or pass null to construct an
        /// instance of the Region class</param>
        /// <param name="declaringType">Type type to extract custom attributes from
        /// that can also define the behaviour of the region</param>
        /// <param name="package">Optional package adds a namespace to this region</param>
        IRegionDefinition BuildUpRegion(object regionInstance = null, Type declaringType = null, IPackage package = null);
    }
}
