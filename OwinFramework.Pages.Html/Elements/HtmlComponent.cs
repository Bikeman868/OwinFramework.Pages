using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Elements
{
    /// <summary>
    /// This is a derivative of Component that renders static Html. It is provided
    /// as a convenience to deveopers to avoid them having to define components for
    /// small snipets or Html.
    /// Does not support data binding expressions, for this functionallity use templates.
    /// Does support namespace expansion via {ns}_ placeholders
    /// </summary>
    public class HtmlComponent : Component
    {
        private string _assetName;
        private string _defaultHtml;
        private bool _replaceNamespaces;

        public HtmlComponent(IComponentDependenciesFactory dependencies)
            : base(dependencies)
        {
        }

        public override IEnumerable<PageArea> GetPageAreas()
        {
            return new[] { PageArea.Body };
        }

        protected override string GetCommentName()
        {
            return "static Html component";
        }

        public void Html(string textAssetName, string defaultHtml)
        {
            _assetName = textAssetName;
            _defaultHtml = defaultHtml;
            _replaceNamespaces = _defaultHtml.IndexOf("{ns}_", StringComparison.OrdinalIgnoreCase) >= 0;

            HtmlWriters = new Action<IRenderContext>[] { RenderHtml };
        }

        private void RenderHtml(IRenderContext renderContext)
        {
            var localizedText = Dependencies.AssetManager.GetLocalizedText(renderContext, _assetName, _defaultHtml);

            if (_replaceNamespaces)
                localizedText = localizedText.Replace("{ns}_", Package == null ? string.Empty : Package.NamespaceName);

            renderContext.Html.WriteLine(localizedText);
        }
    }
}
