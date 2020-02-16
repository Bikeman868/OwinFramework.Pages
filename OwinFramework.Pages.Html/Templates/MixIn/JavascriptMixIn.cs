using System.Text;
using System.Linq;
using OwinFramework.Pages.Core.Interfaces.Templates;

namespace OwinFramework.Pages.Html.Templates
{
    public class JavascriptMixIn
    {
        public void AddToTemplate(ITemplateDefinition template, string javascript, bool inPage)
        {
            const char backTick = '`';
            const char quote = '"';
            const char escape = '\\';
            const char newLine = '\n';
            var quoteString = new string(quote, 1);
            var escapedQuoteString = new string(new[] { escape, quote });
            var newLineString = new string(newLine, 1);
            var blankLineString = new string(newLine, 2);
            var escapedNewLineString = new string(new[] { escape, 'n', escape, newLine });

            // TODO: remove blank lines inside back ticks
            // TODO: remove the mime type comment before the opening back tick

            if (javascript.IndexOf(backTick) >= 0)
            {
                var sb = new StringBuilder();
                var s = javascript.Split(backTick);
                for (var i = 0; i < s.Length; i += 2)
                {
                    sb.Append(s[i]);
                    if (i + 1 < s.Length)
                    {
                        sb.Append(quote);
                        sb.Append(s[i + 1]
                            .Trim()
                            .Replace(blankLineString, newLineString)
                            .Replace(quoteString, escapedQuoteString)
                            .Replace(newLineString, escapedNewLineString));
                        sb.Append(quote);
                    }
                }
                javascript = sb.ToString();
            }

            if (inPage)
            {
                var lines = javascript.Split('\n').Where(l => !string.IsNullOrEmpty(l)).ToList();
                if (lines.Count > 0)
                {
                    template.AddElementOpen("script", "type", "text/javascript");
                    template.AddLineBreak();

                    foreach (var line in lines)
                        template.AddInitializationLine(line);

                    template.AddElementClose();
                    template.AddLineBreak();
                }
            }
            else
            {
                template.AddStaticJavascript(javascript);
            }
        }
    }
}
