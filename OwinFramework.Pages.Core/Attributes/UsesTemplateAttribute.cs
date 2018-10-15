using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a region to fill the region with the specified template
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class UsesTemplateAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that defines the template
        /// to place in a region
        /// </summary>
        /// <param name="templatePath">The relative path of the template. This is usually 
        /// the path to the template file, but this behavior is defined by the template
        /// parsing engine that you used to load the template</param>
        public UsesTemplateAttribute(string templatePath)
        {
            TemplatePath = templatePath;
        }

        /// <summary>
        /// The relative path of the template. This is usually the path to the template 
        /// file, but this behavior is defined by the template parsing engine that you 
        /// used to load the template
        /// </summary>
        public string TemplatePath { get; set; }
    }
}
