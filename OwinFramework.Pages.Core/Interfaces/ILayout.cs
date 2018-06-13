namespace OwinFramework.Pages.Core.Interfaces
{
    /// <summary>
    /// Layouts define an arrangement of html containers that can contain components
    /// or other layouts. The layout emits css and JavaScript to make the layout
    /// perform its desired content layout
    /// </summary>
    public interface ILayout : IElement
    {
        /// <summary>
        /// Changes the component that is displayed in a region of the layout
        /// </summary>
        /// <param name="regionName">The name of a region on this layout</param>
        /// <param name="element">The element to render in this region</param>
        void PopulateRegion(string regionName, IElement element);
    }
}
