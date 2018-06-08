using System;

namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// Defines the properties that classes must implement to be pooled and reused
    /// </summary>
    public interface IReusable : IDisposable
    {
        /// <summary>
        /// True if this instance can be reused
        /// </summary>
        bool IsReusable { get; }

        /// <summary>
        /// This flag is set whilst code is running in the Dispose method.
        /// This avoids recusrively re-disposing the object
        /// </summary>
        bool IsDisposing { get; }

        /// <summary>
        /// This flag is set when the object has been disposed
        /// </summary>
        bool IsDisposed { get; }
    }
}
