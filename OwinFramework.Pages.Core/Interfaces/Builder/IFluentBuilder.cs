namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// This can be used to build elements using a fluent syntax
    /// </summary>
    public interface IFluentBuilder: 
        IComponentBuilder,
        IRegionBuilder,
        ILayoutBuilder,
        IPageBuilder,
        IServiceBuilder
    {
    }
}
