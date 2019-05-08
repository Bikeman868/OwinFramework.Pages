using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to populate a layout zone with static html
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ZoneHtmlAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that defines static Html
        /// to place in a zone of a layout
        /// </summary>
        public ZoneHtmlAttribute(string zoneName, string localizationId, string html)
        {
            ZoneName = zoneName;
            Html = html;
            LocalizationId = localizationId;
        }

        /// <summary>
        /// The name of the zone to populate
        /// </summary>
        public string ZoneName { get; set; }

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
