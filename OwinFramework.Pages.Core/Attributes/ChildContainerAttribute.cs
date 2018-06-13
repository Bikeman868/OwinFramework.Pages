using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to regions that bind to a list to define how they enclose each child
    /// Attach this attribute to layouts to define what element type the layout uses to group regions
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ChildContainerAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that defines the opening and
        /// clasing html for a region
        /// </summary>
        /// <param name="tag">The tag to use to enclose the contents of this element</param>
        public ChildContainerAttribute(string tag = "divl")
        {
            Tag = tag;
        }

        /// <summary>
        /// Constructs and initializes an attribute that defines the opening and
        /// clasing html for a region
        /// </summary>
        /// <param name="tag">The tag to use to enclose the contents of this element</param>
        /// <param name="classNames">Css class names to apply</param>
        public ChildContainerAttribute(string tag = "div", params string[] classNames)
        {
            Tag = tag;
            ClassNames = classNames;
        }

        /// <summary>
        /// The name of the region to populate
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// The name of the component to place in this region
        /// </summary>
        public string[] ClassNames { get; set; }
    }
}
