using System;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Defines the injected dependencies of region.They are packaged like this
    /// to avoid changing the constructor signature when new dependencies are added.
    /// </summary>
    public interface IRegionDependencies : IDisposable
    {
        // No dependencies in this version
    }
}
