using System.Threading;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Facilities.Runtime;
using System.Threading.Tasks;

namespace Sample1.Pages
{
    [Description("<p>This is an example of how to add a semi custom page that inherits from the base Page base class</p>")]
    [Example("<a href='/pages/semiCustom.html'>/pages/semiCustom.html</a>")]
    internal class SemiCustomPage : Page
    {
        public SemiCustomPage(IPageDependenciesFactory dependenciesFactory)
            : base(dependenciesFactory)
        {}

        public override IWriteResult WriteTitle(IRenderContext renderContext, IDataContext dataContext)
        {
            renderContext.Html.WriteLine("Page title");
            return WriteResult.ResponseComplete();
        }

        public override IWriteResult WriteHtml(
            IRenderContext renderContext,
            IDataContext dataContext)
        {
            // Save this location in the output buffer
            var begining = renderContext.Html.CreateInsertionPoint();

            // Write a paragraph of text
            renderContext.Html.WriteElement("p", "This is a semi custom page", "class", "body");

            // Use the saved buffer location to write the heading before the paragraph
            // And do this in a separate thread
            var task = Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(10);
                    begining.WriteElement("h1", "Semi Custom", "class", "page-heading");
                    begining.WriteLine();
                });

            return WriteResult.WaitFor(task);
        }
    }
}