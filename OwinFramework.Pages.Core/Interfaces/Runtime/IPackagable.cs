namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// Elements that can belong to a package and have their assets
    /// in a namespace implement this interface
    /// </summary>
    public interface IPackagable
    {
        /// <summary>
        /// Optional package - not all elements have to be packaged
        /// </summary>
        IPackage Package { get; set; }
    }
}
