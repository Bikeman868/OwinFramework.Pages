using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to populate a region with static html
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class RegionTemplateAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that defines a template
        /// to render into a region of a layout
        /// </summary>
        public RegionTemplateAttribute(string region, string templatePath)
        {
            Region = region;
            TemplatePath = templatePath;
        }

        /// <summary>
        /// The name of the region to populate
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// The template to render
        /// </summary>
        public string TemplatePath { get; set; }
    }
}
