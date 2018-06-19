using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.Html.Interfaces
{
    public interface IHtmlConfiguration
    {
        /// <summary>
        /// The html standard to use for writing html
        /// </summary>
        HtmlFormat HtmlFormat { get; }

        /// <summary>
        /// When this is true the html will include comments
        /// </summary>
        bool IncludeComments { get; }

        /// <summary>
        /// When this is true the html will be indented for readability
        /// </summary>
        bool Indented { get; }
    }
}
