using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to regions to define how they enclose their contents
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ContainerAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that defines the opening and
        /// clasing html for a region
        /// </summary>
        /// <param name="tag">The tag to use to enclose the contents of this element</param>
        public ContainerAttribute(string tag = "div")
        {
            Tag = tag;
        }

        /// <summary>
        /// Constructs and initializes an attribute that defines the opening and
        /// clasing html for a region
        /// </summary>
        /// <param name="tag">The tag to use to enclose the contents of this element</param>
        /// <param name="style">Custom css style to apply</param>
        public ContainerAttribute(string tag = "div", string style = "")
        {
            Tag = tag;
            Style = style;
        }

        /// <summary>
        /// Constructs and initializes an attribute that defines the opening and
        /// clasing html for a region
        /// </summary>
        /// <param name="tag">The tag to use to enclose the contents of this element</param>
        /// <param name="style">Custom css style to apply</param>
        /// <param name="classNames">Css class names to apply</param>
        public ContainerAttribute(string tag = "div", string style = "", params string[] classNames)
        {
            Tag = tag;
            Style = style;
            ClassNames = classNames;
        }

        /// <summary>
        /// The name of the region to populate
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// The name of the component to place in this region
        /// </summary>
        public string Style { get; set; }

        /// <summary>
        /// The name of the component to place in this region
        /// </summary>
        public string[] ClassNames { get; set; }
    }
}
