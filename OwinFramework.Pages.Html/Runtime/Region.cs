using OwinFramework.Pages.Core.Interfaces;

namespace OwinFramework.Pages.Html.Runtime
{
    /// <summary>
    /// Base implementation of IRegion. Inheriting from this olass will insulate you
    /// from any future additions to the IRegion interface
    /// </summary>
    public class Region : Element, IRegion
    {
        public virtual IElement Populate(IElement content)
        {
            return this;
        }
    }
}
