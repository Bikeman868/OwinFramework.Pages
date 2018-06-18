using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime.Internal
{
    public class ClonedElement<T> where T: IElement
    {
        public bool IsClone { get { return true; } }

        protected readonly T Parent;
        private AssetDeployment _assetDeployment = AssetDeployment.Inherit;

        protected ClonedElement(T parent)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");

            Parent = parent;
        }

        public AssetDeployment AssetDeployment
        {
            get { return _assetDeployment == AssetDeployment.Inherit ? Parent.AssetDeployment : _assetDeployment; }
            set { _assetDeployment = value; }
        }

        public void Initialize(IInitializationData initializationData)
        {
            var assetDeployment = AssetDeployment == AssetDeployment.Inherit && Parent.Module != null
                ? Parent.Module.AssetDeployment
                : AssetDeployment;

            assetDeployment = assetDeployment == AssetDeployment.Inherit
                ? initializationData.AssetDeployment
                : AssetDeployment;

            if (assetDeployment == AssetDeployment.PerModule && Parent.Module == null)
                assetDeployment = AssetDeployment.PerWebsite;

            initializationData.HasElement(Parent, assetDeployment, Parent.Module);
        }

        public string Name
        {
            get { return Parent.Name; }
            set { throw new InvalidOperationException("You can not name a cloned " + typeof(T).Name); }
        }

        public IPackage Package
        {
            get { return Parent.Package; }
            set { throw new InvalidOperationException("You can not set the package for a cloned " + typeof(T).Name); }
        }

        public IModule Module
        {
            get { return Parent.Module; }
            set { throw new InvalidOperationException("You can not set the module for a cloned " + typeof(T).Name); }
        }

        public virtual IEnumerator<IElement> GetChildren()
        {
            return null;
        }

        public IWriteResult WriteStaticAssets(AssetType assetType, IHtmlWriter writer)
        {
            return Parent.WriteStaticAssets(assetType, writer);
        }

        public IWriteResult WriteDynamicAssets(AssetType assetType, IHtmlWriter writer, bool includeChildren)
        {
            var result = Parent.WriteDynamicAssets(assetType, writer, false);
            if (!includeChildren) return result;

            var children = GetChildren();
            if (children == null) return result;

            try
            {
                while (!result.IsComplete && children.MoveNext())
                {
                    result.Add(children.Current.WriteDynamicAssets(assetType, writer));
                }
            }
            finally
            {
                children.Dispose();
            }

            return result;
        }

        public IWriteResult WriteInitializationScript(IRenderContext renderContext, IDataContext dataContext, bool includeChildren)
        {
            var result = Parent.WriteInitializationScript(renderContext, dataContext, false);

            if (!includeChildren) return result;

            var children = GetChildren();
            if (children == null) return result;

            try
            {
                while (!result.IsComplete && children.MoveNext())
                {
                    result.Add(children.Current.WriteInitializationScript(renderContext, dataContext));
                }
            }
            finally
            {
                children.Dispose();
            }

            return result;
        }

        public IWriteResult WriteTitle(IRenderContext renderContext, IDataContext dataContext, bool includeChildren)
        {
            var result = Parent.WriteTitle(renderContext, dataContext, false);

            if (!includeChildren) return result;

            var children = GetChildren();
            if (children == null) return result;

            try
            {
                while (!result.IsComplete && children.MoveNext())
                {
                    result.Add(children.Current.WriteTitle(renderContext, dataContext));
                }
            }
            finally
            {
                children.Dispose();
            }

            return result;
        }

        public IWriteResult WriteHead(IRenderContext renderContext, IDataContext dataContext, bool includeChildren)
        {
            var result = Parent.WriteHead(renderContext, dataContext, false);

            if (!includeChildren) return result;

            var children = GetChildren();
            if (children == null) return result;

            try
            {
                while (!result.IsComplete && children.MoveNext())
                {
                    result.Add(children.Current.WriteHead(renderContext, dataContext));
                }
            }
            finally
            {
                children.Dispose();
            }

            return result;
        }
    }
}
