namespace OwinFramework.Pages.Core.Interfaces
{
    /// <summary>
    /// A module is a collection of pages that share the same CSS and JavaScript files
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// The unique name of this module
        /// </summary>
        string Name { get; set; }
    }
}
