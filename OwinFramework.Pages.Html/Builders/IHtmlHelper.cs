namespace OwinFramework.Pages.Html.Builders
{
    public interface IHtmlHelper
    {
        string JoinStyles(params string[] cssStyle);
        string[] StyleAttributes(string style, string[] classNames);
    }
}
