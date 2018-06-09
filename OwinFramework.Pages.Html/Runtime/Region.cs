using OwinFramework.Pages.Core.Interfaces;

namespace OwinFramework.Pages.Html.Runtime
{
    /// <summary>
    /// Base implementation of IRegion. Inheriting from this olass will insulate you
    /// from any future additions to the IRegion interface
    /// </summary>
    public class Region : Element, IRegion
    {
        public string ContainerOpen
        {
            get { throw new System.NotImplementedException(); }
        }

        public string ContainerClose
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}
