using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Html.Interfaces;
using OwinFramework.Pages.Core.Interfaces;

namespace OwinFramework.Pages.Html.Builders
{
    internal class HtmlHelper : IHtmlHelper
    {
        public string JoinStyles(params string[] cssStyle)
        {
            if (cssStyle == null || cssStyle.Length == 0)
                return string.Empty;

            return string.Join(" ", cssStyle
                .Where(s => s != null)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .Select(s => s.EndsWith(";") ? s : s + ";"));
        }

        public string[] StyleAttributes(string style, string[] classNames, IPackage package)
        {
            var tagAttributes = new List<string>();

            if (!string.IsNullOrWhiteSpace(style))
            {
                tagAttributes.Add("style");
                tagAttributes.Add(style);
            }

            if (classNames != null && classNames.Length > 0)
            {
                var classes = string.Join(" ",
                    classNames
                        .Select(c =>
                            {
                                if (package == null || string.IsNullOrEmpty(package.NamespaceName))
                                    return c.Replace("{ns}_", "");

                                return c.Replace("{ns}_", package.NamespaceName + "_");
                            })
                        .Select(c => c.Trim().Replace(' ', '-'))
                        .Where(c => !string.IsNullOrEmpty(c)));
                if (classes.Length > 0)
                {
                    tagAttributes.Add("class");
                    tagAttributes.Add(classes);
                }
            }

            return tagAttributes.ToArray();
        }

        public string NamespaceCssSelector(string selector, IPackage package)
        {
            if (package == null || string.IsNullOrEmpty(package.NamespaceName))
                return selector.Replace(".{ns}_", ".");

            return selector.Replace(".{ns}_", "." + package.NamespaceName + "_");
        }
    }
}
