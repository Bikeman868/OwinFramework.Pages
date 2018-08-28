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
    internal abstract class PageElement
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

            Element = element;
            Parent = parent;

            foreach (var pageArea in element.GetPageAreas())
                if (pageArea >= 0 && pageArea < PageArea.MaxValue)
                    _hasPageArea[(int)pageArea] = true;

            pageData.HasElement(element, element.AssetDeployment, element.Module);

            var elementBase = element as Element;
            if (!ReferenceEquals(elementBase, null))
            {
                var dependentComponents = elementBase.GetDependentComponents();
                if (!ReferenceEquals(dependentComponents, null))
                {
                    foreach (var component in dependentComponents)
                        pageData.NeedsComponent(component);
                }
            }
        }

        public override string ToString()
        {
            var description = GetType().DisplayName(TypeExtensions.NamespaceOption.None);

            if (!string.IsNullOrEmpty(Element.Name))
                description += " '" + Element.Name + "'";

            return description;
        }

        public DebugPageElement GetDebugInfo() 
        {
            return PopulateDebugInfo(new DebugPageElement(), 1, 1);
        }

        protected virtual DebugPageElement PopulateDebugInfo(DebugPageElement debugPageElement, int parentDepth, int childDepth)
        {
            debugPageElement.Element = Element;
            debugPageElement.Name = Element.Name;

            if (parentDepth != 0)
                debugPageElement.Parent = Parent.PopulateDebugInfo(new DebugPageElement(), parentDepth - 1, 0);

            if (childDepth != 0)
                debugPageElement.Children = Children
                    .Select(child => child.PopulateDebugInfo(new DebugPageElement(), 0, childDepth - 1))
                    .ToArray();

            return debugPageElement;
        }

        public virtual void BuildDataContext(DataContextBuilder dataContextBuilder)
        {
            for (var i = 0; i < Children.Length; i++)
                Children[i].BuildDataContext(dataContextBuilder);
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
            IDataContextBuilder dataContextBuilder, 
            PageArea pageArea)
        {
            if (!_hasPageArea[(int)pageArea]) return WriteResult.Continue();

#if TRACE
            renderContext.Trace(() => ToString() + " writing " + pageArea);
#endif

            return WritePageAreaInternal(renderContext, dataContextBuilder, pageArea);
        }

        protected abstract IWriteResult WritePageAreaInternal(
            IRenderContext renderContext, 
            IDataContextBuilder dataContextBuilder, 
            PageArea pageArea);
    }
}
