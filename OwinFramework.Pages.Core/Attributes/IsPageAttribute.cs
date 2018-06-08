using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a page that you want to have discovered and 
    /// registered automitically at startup. If your page implements IPage
    /// it works out better if you build and register it using a Package instead
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class IsPageAttribute : IsElementAttributeBase
    {
        /// <summary>
        /// Constructs an attribute that defines a class to be a page
        /// </summary>
        /// <param name="name">The name of the page. Must be unique within a package or null</param>
        /// <param name="dataContext">The name of the context handler to run to establish this
        /// page's data context</param>
        public IsPageAttribute(string name = null, string dataContext = null)
            : base(name, dataContext)
        {
        }
    }
}
