using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to populate a region with static html
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class RegionHtmlAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that defines static Html
        /// to place in a region of a layout
        /// </summary>
        public RegionHtmlAttribute(string region, string localizationId, string html)
        {
            Region = region;
            Html = html;
            LocalizationId = localizationId;
        }

        /// <summary>
        /// The name of the region to populate
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// The default Html to render for unsupported languages
        /// </summary>
        public string Html { get; set; }

        /// <summary>
        /// The identifier to use to find the localized versions of this Html
        /// </summary>
        public string LocalizationId { get; set; }
    }
}
