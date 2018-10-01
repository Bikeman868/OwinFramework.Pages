using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to an element to deploy a static css asset with this element
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class DeployCssAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that specifies a static CSS asset to deploy
        /// </summary>
        /// <param name="selector">A selector for this class, for exampple "p.quotation"</param>
        /// <param name="cssStyle">A valid css style, for example "font-size: 12px;"</param>
        /// <param name="order">CSS rules are rendered in order of increasing order</param>
        public DeployCssAttribute(string selector, string cssStyle, int order = 0)
        {
            CssSelector = selector;
            CssStyle = cssStyle;
            Order = order;
        }

        /// <summary>
        /// The css selector that selects this style
        /// </summary>
        public string CssSelector { get; set; }

        /// <summary>
        /// The css style definition
        /// </summary>
        public string CssStyle { get; set; }

        /// <summary>
        /// CSS rules are rendered in order of increasing Order
        /// </summary>
        public int Order { get; set; }
    }
}
