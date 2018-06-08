using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Facilities.Runtime;

namespace Sample1.Pages
{
    [Description("<p>This is an example of how to add a semi custom page that inherits from the base Page base class</p>")]
    [Example("<a href='/pages/semiCustom.html'>/pages/semiCustom.html</a>")]
    internal class SemiCustomPage : Page
    {
        public override IWriteResult WriteTitle(IRenderContext renderContext, IDataContext dataContext, IHtmlWriter writer)
        {
            writer.WriteLine("Page title");
            return null;
        }

        public override IWriteResult WriteHtml(
            IRenderContext renderContext,
            IDataContext dataContext,
            IHtmlWriter writer)
        {
            var begining = writer.CreateInsertionPoint();

            writer.WriteLine("This is a semi custom page");

            begining.WriteElement("h1", "Semi Custom", "class", "page-heading");
            begining.WriteLine();

            return null;
        }
    }
}