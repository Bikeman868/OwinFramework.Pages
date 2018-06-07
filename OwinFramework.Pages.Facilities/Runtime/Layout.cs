using OwinFramework.Pages.Core.Interfaces;

namespace OwinFramework.Pages.Facilities.Runtime
{
    /// <summary>
    /// Base implementation of ILayout. Inheriting from this olass will insulate you
    /// from any future additions to the ILayout interface
    /// </summary>
    public class Layout : Element, ILayout
    {
    }
}
