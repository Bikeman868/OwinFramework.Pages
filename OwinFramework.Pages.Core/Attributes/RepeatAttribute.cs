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
        /// <param name="repeatType">The type of data to repeat</param>
        /// <param name="repeatScope">Scope name used when setting repeated data in the data context</param>
        /// <param name="tag">The tag to use to enclose the contents of this element</param>
        /// <param name="style">Custom css style to apply</param>
        /// <param name="classNames">Css class names to apply</param>
        public RepeatAttribute(Type repeatType, string repeatScope, string tag, string style, params string[] classNames)
        {
            RepeatType = repeatType;
            RepeatScope = repeatScope;
            Tag = tag;
            Style = style;
            ClassNames = classNames;
        }

        /// <summary>
        /// Constructs and initializes an attribute that defines how the region repeats
        /// </summary>
        /// <param name="repeatType">The type of data to repeat</param>
        /// <param name="repeatScope">Scope name used when setting repeated data in the data context</param>
        /// <param name="tag">The tag to use to enclose the contents of this element</param>
        public RepeatAttribute(Type repeatType, string repeatScope, string tag)
            : this(repeatType, repeatScope, tag, string.Empty, new string[0])
        {
        }

        /// <summary>
        /// Constructs and initializes an attribute that defines how the region repeats
        /// </summary>
        /// <param name="repeatType">The type of data to repeat</param>
        /// <param name="repeatScope">Scope name used when setting repeated data in the data context</param>
        public RepeatAttribute(Type repeatType, string repeatScope)
            : this(repeatType, repeatScope, "div")
        {
        }

        /// <summary>
        /// Constructs and initializes an attribute that defines how the region repeats
        /// </summary>
        /// <param name="repeatType">The type of data to repeat</param>
        public RepeatAttribute(Type repeatType)
            : this(repeatType, string.Empty)
        {
        }

        /// <summary>
        /// The name of the region to populate
        /// </summary>
        public Type RepeatType { get; set; }

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
        /// Optional scope name to use when setting the repeating data into
        /// the data context
        /// </summary>
        public string RepeatScope { get; set; }

        /// <summary>
        /// Optional scope name to use when retrieving the list from the
        /// data context
        /// </summary>
        public string ListScope { get; set; }
    }
}
