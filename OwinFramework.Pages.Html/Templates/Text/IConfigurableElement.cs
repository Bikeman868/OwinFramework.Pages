using System.Collections.Generic;

namespace OwinFramework.Pages.Html.Templates.Text
{
    /// <summary>
    /// This is implemented by document elements that can have attributes
    /// attached to them that change their behaviour or appearance
    /// </summary>
    public interface IConfigurableElement
    {
        IDictionary<string, string> Attributes { get; }
    }
}