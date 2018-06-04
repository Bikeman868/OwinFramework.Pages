namespace OwinFramework.Pages.Core.Interfaces
{
    /// <summary>
    /// All classes that contribute to response rendering inherit from this interface
    /// </summary>
    public interface IElement
    {
        /// <summary>
        /// The unique name of this element for its type within the package
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Optional package - not all elements have to be packaged
        /// </summary>
        IPackage Package { get; set; }
    }
}
