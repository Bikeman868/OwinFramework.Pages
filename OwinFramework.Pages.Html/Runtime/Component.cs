using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;

namespace OwinFramework.Pages.Html.Runtime
{
    /// <summary>
    /// Base implementation of IComponent. Inheriting from this olass will insulate you
    /// from any future additions to the IComponent interface
    /// </summary>
    public class Component : Element, IComponent
    {
        public override ElementType ElementType { get { return ElementType.Component; } }
    }
}
