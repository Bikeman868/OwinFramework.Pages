using dotless.Core.configuration;
using OwinFramework.Pages.Core.Interfaces.Templates;

namespace OwinFramework.Pages.Html.Templates
{
    public class CssMixIn
    {
        public void AddLessToTemplate(ITemplateDefinition template, string less, bool inPage, bool minify)
        {
            var dotlessConfig = new DotlessConfiguration { MinifyOutput = minify };
            var css = dotless.Core.Less.Parse(less, dotlessConfig);
            AddCssToTemplate(template, css, inPage);
        }

        public void AddCssToTemplate(ITemplateDefinition template, string css, bool inPage)
        {
            if (inPage)
            {
                foreach (var line in css.Split('\n'))
                    template.AddStyleLine(line);
            }
            else
            {
                template.AddStaticCss(css);
            }
        }

    }
}
