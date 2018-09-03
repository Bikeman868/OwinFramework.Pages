using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;
using OwinFramework.Pages.Core.Debug;

namespace OwinFramework.Pages.Html.Elements
{
    /// <summary>
    /// Base class for all classes that write content to the page during
    /// page rendering
    /// </summary>
    internal abstract class PageElement: IDebuggable
    {
        protected readonly IElement Element;
        protected readonly PageElement Parent;

        private readonly bool[] _hasPageArea = new bool[(int)PageArea.MaxValue];
        private PageElement[] _children = new PageElement[0];

        protected PageElement[] Children
        {
            get { return _children; }
            set 
            {
                _children = value ?? new PageElement[0];

                for (var i = 0; i < (int)PageArea.MaxValue; i++)
                    SetHasPageArea((PageArea)i, _hasPageArea[i]);
            }
        }

        protected PageElement(
            PageElementDependencies dependencies,
            PageElement parent,
            IElement element,
            IPageData pageData)
        {
            if (element == null) throw new ArgumentNullException("element");
            if (pageData == null) throw new ArgumentNullException("pageData");

            Element = element;
            Parent = parent;

            foreach (var pageArea in element.GetPageAreas())
                if (pageArea >= 0 && pageArea < PageArea.MaxValue)
                    _hasPageArea[(int)pageArea] = true;
        }

        public override string ToString()
        {
            var description = GetType().DisplayName(TypeExtensions.NamespaceOption.None);

            if (!string.IsNullOrEmpty(Element.Name))
                description += " '" + Element.Name + "'";

            return description;
        }

        public DebugInfo GetDebugInfo(int parentDepth, int childDepth)
        {
            return PopulateDebugInfo(new DebugInfo(), parentDepth, childDepth);
        }

        protected virtual DebugInfo PopulateDebugInfo(DebugInfo debugInfo, int parentDepth, int childDepth)
        {
            debugInfo.Instance = this;
            debugInfo.Element = Element;
            debugInfo.Name = Element.Name;

            debugInfo.DataConsumer = new DebugDataConsumer
            {
            };

            if (parentDepth != 0 && debugInfo.Parent == null)
                debugInfo.Parent = Parent.PopulateDebugInfo(new DebugInfo(), parentDepth - 1, 0);

            if (childDepth != 0 && debugInfo.Children == null)
                debugInfo.Children = Children
                    .Select(child => child.PopulateDebugInfo(new DebugInfo(), 0, childDepth - 1))
                    .ToList();

            return debugInfo;
        }

        public virtual IWriteResult WriteStaticCss(ICssWriter writer)
        {
            return Element.WriteStaticCss(writer);
        }

        public virtual IWriteResult WriteStaticJavascript(IJavascriptWriter writer)
        {
            return Element.WriteStaticJavascript(writer);
        }

        protected bool GetHasPageArea(PageArea pageArea)
        {
            return _hasPageArea[(int)pageArea];
        }

        protected void SetHasPageArea(PageArea pageArea, bool value)
        {
            _hasPageArea[(int)pageArea] = value || _children.Any(c => c.GetHasPageArea(pageArea));
        }

        public virtual IWriteResult WriteStyles(ICssWriter writer)
        {
            if (!_hasPageArea[(int)PageArea.Styles]) return WriteResult.Continue();

            var result = Element.WriteInPageStyles(writer, WriteChildren);
            return result.IsComplete ? result : WriteChildren(writer, result);
        }

        protected virtual IWriteResult WriteChildren(ICssWriter writer, IWriteResult writeResult)
        {
            for (var i = 0; !writeResult.IsComplete && i < Children.Length; i++)
                writeResult.Add(Children[i].WriteStyles(writer));
            return writeResult;
        }

        public virtual IWriteResult WriteScripts(IJavascriptWriter writer)
        {
            if (!_hasPageArea[(int)PageArea.Scripts]) return WriteResult.Continue();

            var result = Element.WriteInPageFunctions(writer, WriteChildren);
            return result.IsComplete ? result : WriteChildren(writer, result);
        }

        protected virtual IWriteResult WriteChildren(IJavascriptWriter writer, IWriteResult writeResult)
        {
            for (var i = 0; !writeResult.IsComplete && i < Children.Length; i++)
                writeResult.Add(Children[i].WriteScripts(writer));
            return writeResult;
        }

        public virtual IWriteResult WritePageArea(
            IRenderContext renderContext, 
            PageArea pageArea)
        {
            if (!_hasPageArea[(int)pageArea]) return WriteResult.Continue();

#if TRACE
            renderContext.Trace(() => ToString() + " writing " + pageArea);
#endif

            return WritePageAreaInternal(renderContext, pageArea);
        }

        protected abstract IWriteResult WritePageAreaInternal(
            IRenderContext renderContext, 
            PageArea pageArea);
    }
}
