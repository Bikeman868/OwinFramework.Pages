using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to regions to define how they enclose their contents
    /// is not part of a package
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ContainerAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that defines the opening and
        /// clasing html for a region
        /// </summary>
        /// <param name="openingHtml">The html used to open the container</param>
        /// <param name="closingHtml">The html used to close the container</param>
        public ContainerAttribute(string openingHtml, string closingHtml)
        {
            OpeningHtml = openingHtml;
            ClosingHtml = closingHtml;
        }

        /// <summary>
        /// The name of the region to populate
        /// </summary>
        public string OpeningHtml { get; set; }

        /// <summary>
        /// The name of the component to place in this region
        /// </summary>
        public string ClosingHtml { get; set; }
    }
}
