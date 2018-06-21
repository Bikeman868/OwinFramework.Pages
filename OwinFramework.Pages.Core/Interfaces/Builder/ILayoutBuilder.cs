namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Use the layout builder to construct layouts using a fluent syntax
    /// </summary>
    public interface ILayoutBuilder
    {
        /// <summary>
        /// Starts building a new layout
        /// </summary>
        ILayoutDefinition Layout(IPackage package = null);
    }
}
