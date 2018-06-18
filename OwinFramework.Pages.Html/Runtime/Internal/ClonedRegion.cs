using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime.Internal
{
    public class ClonedRegion : IRegion
    {
        public ElementType ElementType { get { return ElementType.Region; } }
        public bool IsClone { get { return true; } }

        private readonly IRegion _parent;
        private IElement _content;
        private AssetDeployment _assetDeployment = AssetDeployment.Inherit;

        public ClonedRegion(IRegion parent, IElement content)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");

            _parent = parent;
            _content = content;
        }

        public void Populate(IElement content)
        {
            _content = content;
        }

        public IRegion Clone(IElement content)
        {
            return new ClonedRegion(_parent, content);
        }

        public AssetDeployment AssetDeployment
        {
            get { return _assetDeployment == AssetDeployment.Inherit ? Region.AssetDeployment : _assetDeployment; }
            set { _assetDeployment = value; }
        }

        public void Initialize(IInitializationData initializationData)
        {
            var assetDeployment = AssetDeployment == AssetDeployment.Inherit && _parent.Module != null
                ? _parent.Module.AssetDeployment
                : AssetDeployment;

            assetDeployment = assetDeployment == AssetDeployment.Inherit
                ? initializationData.AssetDeployment
                : AssetDeployment;

            if (assetDeployment == AssetDeployment.PerModule && _parent.Module == null)
                assetDeployment = AssetDeployment.PerWebsite;

            initializationData.HasElement(_parent, assetDeployment, _parent.Module);
        }

        string IElement.Name
        {
            get { return _parent.Name; }
            set { throw new InvalidOperationException("You can not name a cloned region"); }
        }

        IPackage IElement.Package
        {
            get { return _parent.Package; }
            set { throw new InvalidOperationException("You can not set the package for a cloned region"); }
        }

        IModule IElement.Module
        {
            get { return _parent.Module; }
            set { throw new InvalidOperationException("You can not set the module for a cloned region"); }
        }

        public IEnumerator<IElement> GetChildren()
        {
            return _content == null ? null : _content.AsEnumerable().GetEnumerator();
        }

        public IWriteResult WriteStaticAssets(AssetType assetType, IHtmlWriter writer)
        {
            return _parent.WriteStaticAssets(assetType, writer);
        }

        public IWriteResult WriteDynamicAssets(AssetType assetType, IHtmlWriter writer, bool includeChildren)
        {
            var result = _parent.WriteDynamicAssets(assetType, writer, false);
            if (includeChildren && _content != null)
                result.Add(_content.WriteDynamicAssets(assetType, writer));
            return result;
        }

        public IWriteResult WriteInitializationScript(IRenderContext renderContext, IDataContext dataContext, bool includeChildren)
        {
            var result = _parent.WriteInitializationScript(renderContext, dataContext, false);
            if (includeChildren && _content != null && !result.IsComplete)
                result.Add(_content.WriteInitializationScript(renderContext, dataContext));
            return result;
        }

        public IWriteResult WriteTitle(IRenderContext renderContext, IDataContext dataContext, bool includeChildren)
        {
            var result = _parent.WriteTitle(renderContext, dataContext, false);
            if (includeChildren && _content != null && !result.IsComplete)
                result.Add(_content.WriteTitle(renderContext, dataContext));
            return result;
        }

        public IWriteResult WriteHead(IRenderContext renderContext, IDataContext dataContext, bool includeChildren)
        {
            var result = _parent.WriteHead(renderContext, dataContext, false);
            if (includeChildren && _content != null && !result.IsComplete)
                result.Add(_content.WriteHead(renderContext, dataContext));
            return result;
        }

        public IWriteResult WriteHtml(IRenderContext renderContext, IDataContext dataContext, bool includeChildren)
        {
            return _parent.WriteHtml(renderContext, dataContext, includeChildren ? _content : null);
        }

        public IWriteResult WriteHtml(IRenderContext renderContext, IDataContext dataContext, IElement content)
        {
            return _parent.WriteHtml(renderContext, dataContext, content);
        }
    }
}
