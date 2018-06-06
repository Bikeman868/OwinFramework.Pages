using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a stand-alone region that
    /// is not part of a package
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class IsRegionAttribute : IsElementAttributeBase
    {
        /// <summary>
        /// Constructs an attribute that defines a class to be a region
        /// </summary>
        /// <param name="name">The name of the region. Must be unique within a package</param>
        /// <param name="dataContext">The name of the context handler to run to establish this
        /// region's data context</param>
        public IsRegionAttribute(string name, string dataContext = null)
            : base(name, dataContext)
        {
        }
    }
}
