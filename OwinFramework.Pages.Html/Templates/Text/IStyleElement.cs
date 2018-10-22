using System.Collections.Generic;

namespace OwinFramework.Pages.Html.Templates.Text
{
    /// <summary>
    /// This interface is implemented by document elements that change the
    /// style of the text. This includes things like <b/> and things that
    /// have style and class attributes like <span></span> and <div></div>
    /// </summary>
    public interface IStyleElement
    {
        /// <summary>
        /// This is used to represent inline styles
        /// </summary>
        IDictionary<string, string> Styles { get; set; }

        /// <summary>
        /// This is a list of the CSS class names for HTML. When parsing
        /// non-html documents this will be null.
        /// </summary>
        string ClassNames { get; set; }
    }
}