using System.Text;
using System.Linq;
using OwinFramework.Pages.Core.Interfaces.Templates;

namespace OwinFramework.Pages.Html.Templates
{
    public class JavascriptMixIn
    {
        public void AddToTemplate(ITemplateDefinition template, string javascript, bool inPage, bool indented)
        {
            const char backTick = '`';
            const char quote = '"';
            const char escape = '\\';
            const char newLine = '\n';
            const char space = ' ';
            const char tab = '\t';
            var quoteString = new string(quote, 1);
            var escapedQuoteString = new string(new[] { escape, quote });
            var newLineString = new string(newLine, 1);
            var blankLineString = new string(newLine, 2);
            var escapedNewLineString = indented ? new string(new[] { escape, 'n', escape, newLine }) : new string(new[] { escape, 'n' });

            javascript = javascript.Replace("\r", "").Replace("\t", "  ");

            if (javascript.IndexOf(backTick) >= 0)
            {
                var sb = new StringBuilder();
                var s = javascript.Split(backTick);

                for (var i = 0; i < s.Length; i += 2)
                {
                    var inQuotes = false;
                    var inComment = false;
                    var inWhitespace = true;
                    var pendingNewLine = false;

                    void append(char c)
                    {
                        if (pendingNewLine)
                        {
                            sb.Append(newLine);
                            pendingNewLine = false;
                        }
                        sb.Append(c);
                    };

                    for (var j = 0; j < s[i].Length; j++)
                    {
                        var c = s[i][j];
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
                                    if (j + 1 < s[i].Length && s[i][j + 1] == '*')
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
                                    if (j + 1 < s[i].Length && s[i][j + 1] == '/')
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
                                    if (indented)
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

                    if (indented && pendingNewLine)
                        sb.Append(newLine);

                    if (i + 1 < s.Length)
                    {
                        var backTickString = s[i + 1]
                            .Trim()
                            .Replace(blankLineString, newLineString)
                            .Replace(quoteString, escapedQuoteString);

                        sb.Append(quote);

                        //if (indented)
                        //{
                            sb.Append(backTickString.Replace(newLineString, escapedNewLineString));
                        //}
                        //else
                        //{
                        //    var startOfLine = true;

                        //    for (var j = 0; j < backTickString.Length; j++)
                        //    {
                        //        var c = backTickString[j];
                        //        switch (c)
                        //        {
                        //            case newLine:
                        //                if (!startOfLine) sb.Append(space);
                        //                startOfLine = true;
                        //                break;
                        //            case space:
                        //            case tab:
                        //                if (!startOfLine)
                        //                    sb.Append(c);
                        //                break;
                        //            default:
                        //                sb.Append(c);
                        //                startOfLine = false;
                        //                break;
                        //        }
                        //    }
                        //}

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
                    template.WriteScriptOpen();

                    foreach (var line in lines)
                        template.AddInitializationLine(line);

                    template.WriteScriptClose();
                }
            }
            else
            {
                template.AddStaticJavascript(javascript);
            }
        }
    }
}
