namespace OwinFramework.Pages.Core.Interfaces
{
    /// <summary>
    /// A region is part of a layout. The region can contain a 
    /// single component or a layout
    /// </summary>
    public interface IRegion : IElement
    {
        /// <summary>
        /// Returns an IElement implementation that is this element
        /// with specific content inside
        /// </summary>
        IRegion Wrap(IElement content);
    }
}
