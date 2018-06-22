using OwinFramework.Pages.Core.Interfaces;
namespace OwinFramework.Pages.Html.Interfaces
{
    public interface IHtmlHelper
    {
        string JoinStyles(params string[] cssStyle);
        string[] StyleAttributes(string style, string[] classNames, IPackage package);
        string NamespaceCssSelector(string selector, IPackage package);
    }
}
