using System;
using System.Threading;
using System.Threading.Tasks;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
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
        {
            TitleFunc = context => "Page title";
        }

        public override IWriteResult WriteBodyArea(IRenderContext context)
        {
            var html = context.Html;

            // Save this location in the output buffer
            var begining = html.CreateInsertionPoint();

            // Write a paragraph of text
            html.WriteElementLine("p", "This is a semi custom page", "class", "normal");

            // Use the saved buffer location to write the heading before the paragraph
            // and do this in a separate thread
            var task = Task.Factory.StartNew(() =>
                {
                    // Simulate a call to a service or database here
                    Thread.Sleep(10);

                    begining.WriteElementLine("h1", "Semi Custom", "class", "page-heading");
                });

            // Write a second paragraph of text
            html.WriteElementLine("p", "My second paragraph of text", "class", "normal");

            return WriteResult.WaitFor(task);
        }

        public override IWriteResult WriteInPageStyles(
            ICssWriter writer, 
            Func<ICssWriter, IWriteResult, IWriteResult> childrenWriter)
        {
            writer.WriteRule(".normal", "background-color: linen; font-size: 12px;");
            writer.WriteRule(".page-heading", "font-size: 16px;");

            return base.WriteInPageStyles(writer, childrenWriter);
        }

        public override IWriteResult WriteHeadArea(IRenderContext context)
        {
            var result = base.WriteHeadArea(context);

            context.Html.WriteUnclosedElement(
                "link", "rel",
                "stylesheet", "href", "https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css");
            context.Html.WriteLine();
            return result;
        }

    }
}