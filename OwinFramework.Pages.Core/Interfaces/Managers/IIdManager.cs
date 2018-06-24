namespace OwinFramework.Pages.Core.Interfaces.Managers
{
    /// <summary>
    /// Allocates unique ID numbers that can be used to link data
    /// structures build during initialization to request specific
    /// mirrors of them
    /// </summary>
    public interface IIdManager
    {
        /// <summary>
        /// Returns a unique ID. Note that this is only called during
        /// initialization, so the number of IDs requested is small
        /// </summary>
        int GetUniqueId();
    }
}
