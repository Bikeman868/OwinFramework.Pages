using System.Threading;
using System.Threading.Tasks;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
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

        public override IWriteResult WriteTitle(IRenderContext renderContext, IDataContext dataContext, bool includeChildren)
        {
            renderContext.Html.WriteLine("Page title");
            return WriteResult.ResponseComplete();
        }

        public override IWriteResult WriteHead(IRenderContext renderContext, IDataContext dataContext, bool includeChildren)
        {
            renderContext.Html.WriteUnclosedElement(
                "link", "rel", 
                "stylesheet", "href", "https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css");
            renderContext.Html.WriteLine();
            return WriteResult.Continue();
        }

        public override IWriteResult WriteDynamicCss(ICssWriter writer, bool includeChildren)
        {
            writer.WriteRule(".normal", "background-color: linen; font-size: 12px;");
            writer.WriteRule(".page-heading", "font-size: 16px;");
            return WriteResult.Continue();
        }

        public override IWriteResult WriteHtml(
            IRenderContext renderContext,
            IDataContext dataContext, 
            bool includeChildren)
        {
            // Save this location in the output buffer
            var begining = renderContext.Html.CreateInsertionPoint();

            // Write a paragraph of text
            renderContext.Html.WriteElement("p", "This is a semi custom page", "class", "normal");
            renderContext.Html.WriteLine();

            // Use the saved buffer location to write the heading before the paragraph
            // and do this in a separate thread
            var task = Task.Factory.StartNew(() =>
                {
                    // Simulate a call to a service or database here
                    Thread.Sleep(10);

                    begining.WriteElement("h1", "Semi Custom", "class", "page-heading");
                    begining.WriteLine();
                });

            // Write a second paragraph of text
            renderContext.Html.WriteElement("p", "My second paragraph of text", "class", "normal");
            renderContext.Html.WriteLine();

            return WriteResult.WaitFor(task);
        }
    }
}