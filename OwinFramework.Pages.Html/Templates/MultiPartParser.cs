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
        private readonly MustacheMixIn _mustacheMixIn;
        private readonly JavascriptMixIn _javascriptMixIn;
        private readonly CssMixIn _cssMixin;

        /// <summary>
        /// Constructs a parser that can parse Markdown documents into templates
        /// </summary>
        public MultiPartParser(
            IStringBuilderFactory stringBuilderFactory,
            ITemplateBuilder templateBuilder)
        {
            _stringBuilderFactory = stringBuilderFactory;
            _templateBuilder = templateBuilder;

            _mustacheMixIn = new MustacheMixIn();
            _javascriptMixIn = new JavascriptMixIn();
            _cssMixin = new CssMixIn();
        }

        public ITemplate Parse(TemplateResource[] resources, IPackage package, IModule module)
        {
            var templateDefinition = _templateBuilder
                .BuildUpTemplate()
                .PartOf(package)
                .DeployIn(module);

            foreach (var resource in resources)
                ParseResource(resource, templateDefinition, package);

            return templateDefinition.Build();
        }

        private void ParseResource(
            TemplateResource resource, 
            ITemplateDefinition template, 
            IPackage package)
        {
            var encoding = resource.Encoding ?? Encoding.UTF8;
            var text = encoding.GetString(resource.Content);

            var section = "--html";
            var newSection = true;
            var blankLineCount = 0;

            var lines = text
                .Replace("\r", string.Empty)
                .Split('\n')
                .Select(s => s.Trim());

            var scriptLines = new List<string>();
            var cssLines = new List<string>();
            var lessLines = new List<string>();
            var htmlLines = new List<string>();
            var headLines = new List<string>();
            var initLines = new List<string>();

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
                    for (var i = 0; i < blankLineCount - 1; i++)
                        scriptLines.Add(string.Empty);

                    scriptLines.Add(line);
                }
                else if (section.StartsWith("--css"))
                {
                    for (var i = 0; i < blankLineCount - 1; i++)
                        cssLines.Add(string.Empty);

                    cssLines.Add(line);
                }
                else if (section.StartsWith("--less"))
                {
                    for (var i = 0; i < blankLineCount - 1; i++)
                        lessLines.Add(string.Empty);

                    lessLines.Add(line);
                }
                else if (section.StartsWith("--head"))
                {
                    for (var i = 0; i < blankLineCount - 1; i++)
                        headLines.Add(string.Empty);

                    headLines.Add(line);
                }
                else if (section.StartsWith("--init"))
                {
                    for (var i = 0; i < blankLineCount - 1; i++)
                        initLines.Add(string.Empty);

                    initLines.Add(line);
                }
                else if (section.StartsWith("--html"))
                {
                    for (var i = 0; i < blankLineCount - 1; i++)
                        htmlLines.Add(string.Empty);

                    htmlLines.Add(line);
                }

                newSection = false;
                blankLineCount = 0;
            }

            foreach (var headLine in headLines)
                template.AddHtml(PageArea.Head, headLine);

            if (scriptLines.Count > 0)
                _javascriptMixIn.AddToTemplate(template, string.Join("\n", scriptLines), true, true);

            if (cssLines.Count > 0)
                _cssMixin.AddCssToTemplate(template, string.Join("\n", cssLines), true);

            if (lessLines.Count > 0)
                _cssMixin.AddLessToTemplate(template, string.Join("\n", lessLines), true, true);

            foreach (var htmlLine in htmlLines)
            {
                template.AddHtml(PageArea.Body, htmlLine);
                template.AddLineBreak(PageArea.Body);
            }

            foreach (var initLine in initLines)
                template.AddHtml(PageArea.Initialization, initLine);
        }
    }
}
