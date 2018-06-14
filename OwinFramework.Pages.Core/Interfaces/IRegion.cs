namespace OwinFramework.Pages.Core.Interfaces
{
    /// <summary>
    /// A region is part of a layout. The region can contain a 
    /// single component or a layout
    /// </summary>
    public interface IRegion : IElement
    {
        /// <summary>
        /// Constructs an element that is the result of puttting the
        /// supplied element inside this region. The supplied element 
        /// should be either a component or a Layout
        /// </summary>
        IElement Populate(IElement content);
    }
}
