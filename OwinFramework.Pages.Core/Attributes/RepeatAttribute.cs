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
        /// <param name="childTag">The tag to use to enclose each repeated child element</param>
        /// <param name="childStyle">Custom css style to apply to all child elements</param>
        /// <param name="childClassNames">Css class names to apply to all child elements. Use {ns}_ prefix to prepend package namespace</param>
        public RepeatAttribute(Type repeatType, string repeatScope, string childTag, string childStyle, params string[] childClassNames)
        {
            RepeatType = repeatType;
            RepeatScope = repeatScope;
            ChildTag = childTag;
            ChildStyle = childStyle;
            ChildClassNames = childClassNames;
        }

        /// <summary>
        /// Constructs and initializes an attribute that defines how the region repeats
        /// </summary>
        /// <param name="repeatType">The type of data to repeat</param>
        /// <param name="repeatScope">Scope name used when setting repeated data in the data context</param>
        /// <param name="childTag">The tag to use to enclose each repeated child element</param>
        public RepeatAttribute(Type repeatType, string repeatScope, string childTag)
            : this(repeatType, repeatScope, childTag, string.Empty, new string[0])
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
        /// The type of data that should be repeated for each child element
        /// </summary>
        public Type RepeatType { get; set; }

        /// <summary>
        /// The tag to use to enclose each repeated child element
        /// </summary>
        public string ChildTag { get; set; }

        /// <summary>
        /// Custom css style to apply to all child elements
        /// </summary>
        public string ChildStyle { get; set; }

        /// <summary>
        /// Css class names to apply to all child elements. Use {ns}_ prefix to prepend package namespace
        /// </summary>
        public string[] ChildClassNames { get; set; }

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
