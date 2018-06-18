using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Specifies the content of a component as localized text
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class RenderHtmlAttribute: Attribute
    {
        /// <summary>
        /// Constructs an attribute that provides a description of this element
        /// </summary>
        /// <param name="textName">The name of this localized string</param>
        /// <param name="html">The default language version of this string in html 
        /// format - can be translated in the text manager</param>
        public RenderHtmlAttribute(string textName, string html = null)
        {
            TextName = textName;
            Html = html;
        }

        /// <summary>
        /// Constructs an attribute that provides a description of this element
        /// </summary>
        /// <param name="textName">The name of this localized string</param>
        /// <param name="order">The order of rendering the html</param>
        /// <param name="html">The default language version of this string in html 
        /// format - can be translated in the text manager</param>
        public RenderHtmlAttribute(string textName, int order, string html = null)
        {
            TextName = textName;
            Order = order;
            Html = html;
        }

        /// <summary>
        /// The name of this text in the text manager. Allows other localized
        /// versions of the text to be available
        /// </summary>
        public string TextName { get; set; }

        /// <summary>
        /// A fragment of HTML that describes this element
        /// </summary>
        public string Html { get; set; }

        /// <summary>
        /// Defines the order to render the html paragraphs. The order of
        /// attributes is not preserved by .Net
        /// </summary>
        public int Order { get; set; }
    }
}
