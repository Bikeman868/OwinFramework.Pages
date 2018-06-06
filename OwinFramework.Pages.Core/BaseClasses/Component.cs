using OwinFramework.Pages.Core.Interfaces;

namespace OwinFramework.Pages.Core.BaseClasses
{
    /// <summary>
    /// Base implementation of IComponent. Inheriting from this olass will insulate you
    /// from any future additions to the IComponent interface
    /// </summary>
    public class Component : Element, IComponent
    {
    }
}
