using OwinFramework.Pages.Core.Enums;
using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach these attributes to elements to render the contents of the templates
    /// into the page when the element is rendered
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class RenderTemplateAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that defines the template
        /// to place in a region
        /// </summary>
        /// <param name="templatePath">The relative path of the template. This is usually 
        /// the path to the template file, but this behavior is defined by the template
        /// parsing engine that you used to load the template</param>
        /// <param name="pageArea">The area of the page to render the template into. You
        /// can only render one template to each page area</param>
        public RenderTemplateAttribute(string templatePath, PageArea pageArea = PageArea.Body)
        {
            TemplatePath = templatePath;
            PageArea = pageArea;
        }

        /// <summary>
        /// The relative path of the template. This is usually the path to the template 
        /// file, but this behavior is defined by the template parsing engine that you 
        /// used to load the template
        /// </summary>
        public string TemplatePath { get; set; }

        /// <summary>
        /// The area of the page where you would like this template to be rendered. You can
        /// only render one template in each area of the page. To render multiple templates
        /// you must add multiple elements to the page
        /// </summary>
        public PageArea PageArea { get; set; }
    }
}
