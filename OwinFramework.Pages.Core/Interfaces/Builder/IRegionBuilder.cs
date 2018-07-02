using System;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Use the region builder to construct regions using a fluent syntax
    /// </summary>
    public interface IRegionBuilder
    {
        /// <summary>
        /// Starts building a new region
        /// </summary>
        IRegionDefinition Region(Type declaringType = null, IPackage package = null);
    }
}
