using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Templates;
using OwinFramework.Pages.Html.Templates.Text;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Templates
{
    /// <summary>
    /// This is a template parser that allows you to combine Html, JavaScript and
    /// CSS into a signle file. The Html will be written to the body of the page
    /// and the JavaScript and CSS will be deployed as assets.
    /// To identify the sections of the file start a line with two hyphens and the
    /// content type, for example "--css", "--javascript" or "--html"
    /// </summary>
    public class MultiPartParser: DocumentParser, ITemplateParser
    {
        private readonly IStringBuilderFactory _stringBuilderFactory;
        private readonly ITemplateBuilder _templateBuilder;

        /// <summary>
        /// Constructs a parser that can parse Markdown documents into templates
        /// </summary>
        public MultiPartParser(
            IStringBuilderFactory stringBuilderFactory,
            ITemplateBuilder templateBuilder)
        {
            _stringBuilderFactory = stringBuilderFactory;
            _templateBuilder = templateBuilder;
        }

        public ITemplate Parse(byte[] template, Encoding encoding, IPackage package)
        {
            encoding = encoding ?? Encoding.UTF8;
            var text = encoding.GetString(template);

            var templateDefinition = _templateBuilder.BuildUpTemplate();

            var section = "--html";
            var newSection = true;
            var blankLineCount = 0;

            var lines = text
                .Replace("\r", string.Empty)
                .Split('\n')
                .Select(s => s.Trim());

            foreach (var line in lines)
            {
                if (line.StartsWith("--"))
                {
                    section = line.ToLower().Trim();
                    newSection = true;
                    blankLineCount = 0;
                    continue;
                }

                if (line.Length == 0)
                {
                    if (!newSection)
                        blankLineCount++;
                    continue;
                }


                if (section.StartsWith("--javascript"))
                {
                    for (var i = 0; i < blankLineCount; i++)
                        templateDefinition.AddScriptLine(string.Empty);

                    templateDefinition.AddScriptLine(line);
                }
                else if (section.StartsWith("--css"))
                {
                    for (var i = 0; i < blankLineCount; i++)
                        templateDefinition.AddStyleLine(string.Empty);

                    templateDefinition.AddStyleLine(line);
                }
                else if (section.StartsWith("--head"))
                {
                    for (var i = 0; i < blankLineCount; i++)
                        templateDefinition.AddHeadLine(string.Empty);

                    templateDefinition.AddHeadLine(line);
                }
                else if (section.StartsWith("--init"))
                {
                    for (var i = 0; i < blankLineCount; i++)
                        templateDefinition.AddInitializationLine(string.Empty);

                    templateDefinition.AddInitializationLine(line);
                }
                else if (section.StartsWith("--html"))
                {
                    for (var i = 0; i < blankLineCount; i++)
                        templateDefinition.AddLineBreak();

                    templateDefinition.AddHtml(line);
                    templateDefinition.AddLineBreak();
                }

                newSection = false;
                blankLineCount = 0;
            }

            return templateDefinition.Build();
        }
    }
}
