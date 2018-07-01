﻿using System.Threading;
using System.Threading.Tasks;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;

namespace Sample1.SamplePages
{
    [Description("<p>This is an example of how to add a semi custom page that inherits from the base Page base class</p>")]
    [Example("<a href='/pages/semiCustom.html'>/pages/semiCustom.html</a>")]
    internal class SemiCustomPage : Page
    {
        public SemiCustomPage(IPageDependenciesFactory dependenciesFactory)
            : base(dependenciesFactory)
        {}

        public override IWriteResult WriteTitle(IRenderContext context, bool includeChildren)
        {
            context.Html.WriteLine("Page title");
            return WriteResult.ResponseComplete();
        }

        public override IWriteResult WriteHead(IRenderContext context, bool includeChildren)
        {
            context.Html.WriteUnclosedElement(
                "link", "rel", 
                "stylesheet", "href", "https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css");
            context.Html.WriteLine();
            return WriteResult.Continue();
        }

        public override IWriteResult WriteDynamicCss(ICssWriter writer, bool includeChildren)
        {
            writer.WriteRule(".normal", "background-color: linen; font-size: 12px;");
            writer.WriteRule(".page-heading", "font-size: 16px;");
            return WriteResult.Continue();
        }

        public override IWriteResult WriteHtml(
            IRenderContext context,
            bool includeChildren)
        {
            // Save this location in the output buffer
            var begining = context.Html.CreateInsertionPoint();

            // Write a paragraph of text
            context.Html.WriteElementLine("p", "This is a semi custom page", "class", "normal");

            // Use the saved buffer location to write the heading before the paragraph
            // and do this in a separate thread
            var task = Task.Factory.StartNew(() =>
                {
                    // Simulate a call to a service or database here
                    Thread.Sleep(10);

                    begining.WriteElementLine("h1", "Semi Custom", "class", "page-heading");
                });

            // Write a second paragraph of text
            context.Html.WriteElementLine("p", "My second paragraph of text", "class", "normal");

            return WriteResult.WaitFor(task);
        }
    }
}