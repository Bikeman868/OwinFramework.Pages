namespace OwinFramework.Pages.Html.Runtime
{
    /// <summary>
    /// The IoC dependencies are wrapped in this factory so that when
    /// new dependencies are added it does not change the constructor
    /// which would break any application code that inherits from it
    /// </summary>
    public interface IComponentDependenciesFactory
    {
        IComponentDependencies Create();
    }
}
