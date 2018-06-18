using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    /// <summary>
    /// Base implementation of ILayout. Inheriting from this olass will insulate you
    /// from any future additions to the ILayout interface
    /// You can also use this class directly but it provides only minimal region 
    /// functionallity
    /// </summary>
    public class Layout : Element, ILayout
    {
        public override ElementType ElementType { get { return ElementType.Layout; } }
        public bool IsClone { get { return false; } }

        protected IThreadSafeDictionary<string, IElement> Content;

        /// <summary>
        /// Do not change this constructor signature, it will break application
        /// classes that inherit from this class. Add dependencies to
        /// ILayoutDependenciesFactory and ILayoutDependencies
        /// </summary>
        public Layout(ILayoutDependenciesFactory layoutDependenciesFactory)
        {
            Content = layoutDependenciesFactory.DictionaryFactory.Create<string, IElement>(StringComparer.InvariantCultureIgnoreCase);
        }

        public void Populate(string regionName, IElement element)
        {
            Content[regionName] = element;
        }

        public ILayout Clone()
        {
            return new ClonedLayout(this);
        }

        public override IEnumerator<IElement> GetChildren()
        {
            return Content.Values.GetEnumerator();
        }

        public virtual IWriteResult WriteHtml(
            IRenderContext renderContext, 
            IDataContext dataContext, 
            Func<string, IElement> contentFunc)
        {
            var result = WriteResult.Continue();
            if (Content.Count == 0) return result;

            using (var regionNames = Content.KeysLocked)
            {
                foreach(var regionName in regionNames)
                {
                    var element = contentFunc(regionName) ?? Content[regionName];
                    if (element != null)
                        result.Add(element.WriteHtml(renderContext, dataContext));
                }
            }
            return result;
        }
    }
}
