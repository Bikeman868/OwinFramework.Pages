using OwinFramework.Pages.Core.Interfaces;

namespace OwinFramework.Pages.Html.Runtime
{
    /// <summary>
    /// Base implementation of ILayout. Inheriting from this olass will insulate you
    /// from any future additions to the ILayout interface
    /// </summary>
    public abstract class Layout : Element, ILayout
    {
        public abstract void PopulateRegion(string regionName, IElement element);
    }
}
