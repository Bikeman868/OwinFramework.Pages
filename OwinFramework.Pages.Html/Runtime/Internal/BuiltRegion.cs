using System;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime.Internal
{
    public class BuiltRegion : Region
    {
        public Action<IHtmlWriter> WriteOpen;
        public Action<IHtmlWriter> WriteClose;

        public BuiltRegion(IRegionDependenciesFactory regionDependenciesFactory)
            : base(regionDependenciesFactory)
        { }

        public override IWriteResult WriteHtml(
            IRenderContext renderContext,
            IDataContext dataContext, 
            IElement content)
        {
            WriteOpen(renderContext.Html);

            if (content != null)
            {
                // TODO: if data bound repeat content for each item on data bound list

                var result = content.WriteHtml(renderContext, dataContext);
                WriteClose(renderContext.Html);
                return result;
            }

            WriteClose(renderContext.Html);
            return WriteResult.Continue();
        }
    }
}
