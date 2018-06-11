using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to an element to define a css style for it
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class StyleAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that specifies the style of
        /// this element
        /// </summary>
        /// <param name="cssStyle">A valid css style, for example "font-size: 12px;"</param>
        public StyleAttribute(string cssStyle)
        {
            CssStyle = cssStyle;
        }

        /// <summary>
        /// The css style definition
        /// </summary>
        public string CssStyle { get; set; }
    }
}
