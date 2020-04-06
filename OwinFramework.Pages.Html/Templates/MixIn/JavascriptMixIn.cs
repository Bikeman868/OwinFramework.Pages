using System.Text;
using System.Linq;
using OwinFramework.Pages.Core.Interfaces.Templates;
using System;
using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.Html.Templates
{
    public class JavascriptMixIn
    {
        const char backTick = '`';
        const char quote = '"';
        const char escape = '\\';
        const char newLine = '\n';
        const char space = ' ';
        const char tab = '\t';

        private readonly string quoteString;
        private readonly string escapedQuoteString;
        private readonly string newLineString;
        private readonly string blankLineString;
        private string escapedNewLineString;

        private readonly MustacheMixIn _mustacheMixIn;

        public JavascriptMixIn()
        {
            quoteString = new string(quote, 1);
            escapedQuoteString = new string(new[] { escape, quote });
            newLineString = new string(newLine, 1);
            blankLineString = new string(newLine, 2);
            _mustacheMixIn = new MustacheMixIn();
        }

        public void AddToTemplate(ITemplateDefinition template, string javascript, bool inPage, bool indented)
        {
            escapedNewLineString = indented
                ? new string(new[] { escape, 'n', escape, newLine })
                : new string(new[] { escape, 'n' });

            javascript = javascript.Replace("\r", "").Replace("\t", "  ");

            var context = new Context
            {
                Template = template,
                Indented = indented,
                InPage = inPage
            };

            if (javascript.IndexOf(backTick) >= 0)
            {
                var backTickSections = javascript.Split(backTick);

                for (var i = 0; i < backTickSections.Length; i += 2)
                {
                    AppendJavascriptSection(context, backTickSections[i]);

                    if (backTickSections.Length > i + 1)
                    {
                        if (backTickSections[i].EndsWith("/*mustache*/"))
                            AppendMustacheSection(context, backTickSections[i + 1]);
                        else if (backTickSections[i].EndsWith("/*html*/"))
                            AppendHtmlSection(context, backTickSections[i + 1]);
                        else
                            throw new NotImplementedException(
                                "Opening back-ticks must be preceeded by a Javascript comment that identifies " +
                                "the type of processing to perform on the back-tick content. Supported comments" +
                                "in this version are /*html*/ and /*mustache*/");
                    }
                }
            }
            else
            {
                AppendJavascriptSection(context, javascript);
            }
        }

        private void AppendScript(Context context, string script)
        {
            if (string.IsNullOrEmpty(script))
                return;

            if (context.InPage)
            {
                var lines = script.Split('\n').Where(l => l != null).ToList();
                if (lines.Count > 0)
                {
                    context.EnsureScriptOpen();

                    for (var i = 0; i < lines.Count; i++)
                    {
                        var line = lines[i];
                        if (!string.IsNullOrEmpty(line) || i < lines.Count - 1)
                        {
                            context.Template.AddHtml(PageArea.Initialization, line);
                            if (i < lines.Count - 1)
                                context.Template.AddLineBreak(PageArea.Initialization);
                        }
                    }
                }
            }
            else
            {
                context.Template.AddStaticJavascript(script);
            }
        }

        private void AppendMustacheSection(Context context, string s)
        {
            if (!context.InPage)
                throw new NotImplementedException(
                    "You can only use mustache data binding expressions in Javascript that is " +
                    "rendered into the page. It makes no sense to create static Javascript " +
                    "assets with data binding because the Javascript is static by definition.");

            context.EnsureScriptOpen();
            _mustacheMixIn.AddToTemplate(context.Template, PageArea.Initialization, s);
        }

        private void AppendHtmlSection(Context context, string s)
        {
            var escapedString = s
                .Trim()
                .Replace(blankLineString, newLineString)
                .Replace(quoteString, escapedQuoteString)
                .Replace(newLineString, escapedNewLineString);

            AppendScript(context, quote + escapedString + quote);
        }

        private void AppendJavascriptSection(Context context, string s)
        {
            var inQuotes = false;
            var inComment = false;
            var inWhitespace = true;
            var pendingNewLine = false;

            var sb = new StringBuilder();

            void append(char c)
            {
                if (pendingNewLine)
                {
                    sb.Append(newLine);
                    pendingNewLine = false;
                }
                sb.Append(c);
            };

            for (var j = 0; j < s.Length; j++)
            {
                var c = s[j];
                switch (c)
                {
                    case quote:
                        if (inComment) break;
                        inQuotes = !inQuotes;
                        inWhitespace = false;
                        append(c);
                        break;
                    case '/':
                        if (inComment) break;
                        if (inQuotes)
                            append(c);
                        else
                        {
                            if (j + 1 < s.Length && s[j + 1] == '*')
                            {
                                inComment = true;
                                j++;
                            }
                            else
                            {
                                inWhitespace = false;
                                append(c);
                            }
                        }
                        break;
                    case '*':
                        if (inComment)
                        {
                            if (j + 1 < s.Length && s[j + 1] == '/')
                            {
                                inComment = false;
                                j++;
                            }
                        }
                        else
                        {
                            inWhitespace = false;
                            append(c);
                        }
                        break;
                    case newLine:
                        if (inComment) break;
                        inWhitespace = true;
                        pendingNewLine = true;
                        break;
                    case space:
                    case tab:
                        if (inComment) break;
                        if (inQuotes)
                        {
                            append(c);
                            break;
                        }
                        if (inWhitespace)
                        {
                            if (context.Indented)
                                append(space);
                        }
                        else
                        {
                            inWhitespace = true;
                            append(space);
                        }
                        break;
                    default:
                        if (inComment) break;
                        inWhitespace = false;
                        append(c);
                        break;
                }
            }

            if (context.Indented && pendingNewLine)
                sb.Append(newLine);

            AppendScript(context, sb.ToString());
        }

        private class Context
        {
            public ITemplateDefinition Template;
            public bool InPage;
            public bool Indented;
            public bool HasOutput;

            public void EnsureScriptOpen()
            {
                if (!HasOutput)
                {
                    HasOutput = true;
                    Template.WriteScriptOpen(PageArea.Initialization);
                }
            }
        }
    }
}
