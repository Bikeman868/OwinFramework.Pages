﻿using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class ClonedElement<T> where T: IElement
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
            var assetDeployment = AssetDeployment == AssetDeployment.Inherit && Module != null
                ? Module.AssetDeployment
                : AssetDeployment;

            assetDeployment = assetDeployment == AssetDeployment.Inherit
                ? initializationData.AssetDeployment
                : assetDeployment;

            if (assetDeployment == AssetDeployment.PerModule && Module == null)
                assetDeployment = AssetDeployment.PerWebsite;

            initializationData.HasElement(Parent, assetDeployment, Parent.Module);

            var children = GetChildren();
            if (children == null) return;

            if (AssetDeployment != AssetDeployment.Inherit)
            {
                initializationData.Push();
                initializationData.AssetDeployment = AssetDeployment;
            }
            try
            {
                while (children.MoveNext())
                {
                    children.Current.Initialize(initializationData);
                }
            }
            finally
            {
                if (AssetDeployment != AssetDeployment.Inherit)
                {
                    initializationData.Pop();
                }
                children.Dispose();
            }
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

        public IWriteResult WriteStaticCss(ICssWriter writer)
        {
            return Parent.WriteStaticCss(writer);
        }

        public IWriteResult WriteStaticJavascript(IJavascriptWriter writer)
        {
            return Parent.WriteStaticJavascript(writer);
        }

        public IWriteResult WriteDynamicCss(ICssWriter writer, bool includeChildren = true)
        {
            var result = Parent.WriteDynamicCss(writer, false);
            if (!includeChildren) return result;

            var children = GetChildren();
            if (children == null) return result;

            try
            {
                while (!result.IsComplete && children.MoveNext())
                {
                    result.Add(children.Current.WriteDynamicCss(writer));
                }
            }
            finally
            {
                children.Dispose();
            }

            return result;
        }

        public IWriteResult WriteDynamicJavascript(IJavascriptWriter writer, bool includeChildren = true)
        {
            var result = Parent.WriteDynamicJavascript(writer, false);
            if (!includeChildren) return result;

            var children = GetChildren();
            if (children == null) return result;

            try
            {
                while (!result.IsComplete && children.MoveNext())
                {
                    result.Add(children.Current.WriteDynamicJavascript(writer));
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
