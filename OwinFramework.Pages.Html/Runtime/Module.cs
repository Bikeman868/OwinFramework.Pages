using OwinFramework.Pages.Core.Interfaces;

namespace OwinFramework.Pages.Html.Runtime
{
    /// <summary>
    /// Base implementation of IModule. Inheriting from this class will insulate you
    /// from any future additions to the IModule interface
    /// </summary>
    public class Module : IModule
    {
        public string Name { get; set; }
    }
}
