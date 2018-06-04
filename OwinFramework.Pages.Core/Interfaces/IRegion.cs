namespace OwinFramework.Pages.Core.Interfaces
{
    /// <summary>
    /// A region is part of a layout. The region can contain a 
    /// single component or a layout
    /// </summary>
    public interface IRegion : IElement
    {
        /// <summary>
        /// Returns the html to output before the contents of the region
        /// </summary>
        string ContainerOpen { get; }

        /// <summary>
        /// Returns the html to output after the contents of the region
        /// </summary>
        string ContainerClose { get; }
    }
}
