using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class BuiltComponent : Component
    {
        public List<Action<IRenderContext>> HtmlWriters;
        public List<Action<IHtmlWriter>> StyleAssets;
        public List<Action<IHtmlWriter>> FunctionAssets;

        public BuiltComponent(IComponentDependenciesFactory dependencies)
            : base(dependencies)
        { }

        public override IWriteResult WriteHtml(IRenderContext renderContext, IDataContext dataContext, bool includeChildren)
        {
            if (renderContext.IncludeComments)
                renderContext.Html.WriteComment(
                    (string.IsNullOrEmpty(Name) ? "unnamed" : Name) +
                    (Package == null ? " component" : " component from the " + Package.Name + " package"));

            if (HtmlWriters != null)
            {
                foreach (var writer in HtmlWriters)
                    writer(renderContext);
            }

            return WriteResult.Continue();
        }

        public override IWriteResult WriteStaticAssets(AssetType assetType, IHtmlWriter writer)
        {
            List<Action<IHtmlWriter>> assets = null;
            var commentStyle = CommentStyle.SingleLineC;

            if (assetType == AssetType.Style)
            {
                assets = StyleAssets;
                commentStyle = CommentStyle.MultiLineC;
            }
            else if (assetType == AssetType.Script)
            {
                assets = FunctionAssets;
            }

            if (assets != null && assets.Count > 0)
            {
                writer.WriteComment(
                        assetType + " assets for " +
                        (string.IsNullOrEmpty(Name) ? "unnamed" : Name) +
                        (Package == null ? " component" : " component from the " + Package.Name + " package"),
                        commentStyle);

                foreach (var asset in assets)
                    asset(writer);
            }

            return WriteResult.Continue();
        }
    }
}
