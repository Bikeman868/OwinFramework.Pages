using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to populate a layout zone with static html
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ZoneTemplateAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that defines a template
        /// to render into a zone of a layout
        /// </summary>
        public ZoneTemplateAttribute(string zoneName, string templatePath)
        {
            ZoneName = zoneName;
            TemplatePath = templatePath;
        }

        /// <summary>
        /// The name of the zoneName to populate
        /// </summary>
        public string ZoneName { get; set; }

        /// <summary>
        /// The template to render
        /// </summary>
        public string TemplatePath { get; set; }
    }
}
