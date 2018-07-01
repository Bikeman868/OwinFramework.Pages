using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a region to make it repeat for each item on a list
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class RepeatAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that defines how the region repeats
        /// </summary>
        /// <param name="itemType">The type of data to repeat</param>
        /// <param name="tag">The tag to use to enclose the contents of this element</param>
        /// <param name="style">Custom css style to apply</param>
        /// <param name="classNames">Css class names to apply</param>
        public RepeatAttribute(Type itemType, string tag = "div", string style = "", params string[] classNames)
        {
            ItemType = itemType;
            Tag = tag;
            Style = style;
            ClassNames = classNames;
        }

        /// <summary>
        /// The name of the region to populate
        /// </summary>
        public Type ItemType { get; set; }

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

        /// <summary>
        /// Optional scope name to allow grandchildren to bind to this
        /// data even if the children add context data of the same type
        /// </summary>
        public string ScopeName { get; set; }
    }
}
