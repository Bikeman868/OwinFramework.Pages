using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Builders;
using OwinFramework.Pages.Html.Runtime.Internal;

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
        private readonly ILayoutDependenciesFactory _layoutDependenciesFactory;
        public override ElementType ElementType { get { return ElementType.Layout; } }
        public bool IsClone { get { return false; } }

        protected IThreadSafeDictionary<string, IRegion> Content;

        /// <summary>
        /// Do not change this constructor signature, it will break application
        /// classes that inherit from this class. Add dependencies to
        /// ILayoutDependenciesFactory and ILayoutDependencies
        /// </summary>
        public Layout(ILayoutDependenciesFactory layoutDependenciesFactory)
        {
            _layoutDependenciesFactory = layoutDependenciesFactory;
            Content = layoutDependenciesFactory.DictionaryFactory.Create<string, IRegion>(StringComparer.InvariantCultureIgnoreCase);
        }

        public IRegion GetRegion(string regionName)
        {
            IRegion region;
            if (!Content.TryGetValue(regionName, out region))
                throw new Exception("Layout doe not have a '" + regionName + "' region");
            return region;
        }

        public void Populate(string regionName, IElement element)
        {
            var region = GetRegion(regionName);
            if (region.IsClone)
                region.Populate(element);
            else
            {
                region = region.Clone(element);
                Content[regionName] = region;
            }
        }

        public ILayout Clone()
        {
            using (var regionNames = Content.KeysLocked)
                return new ClonedLayout(_layoutDependenciesFactory, this, regionNames);
        }

        public override IEnumerator<IElement> GetChildren()
        {
            return Content.Values.GetEnumerator();
        }

        public virtual IWriteResult WriteHtml(
            IRenderContext renderContext, 
            IDataContext dataContext,
            Func<string, IRegion> regionLookup)
        {
            var result = WriteResult.Continue();
            if (regionLookup == null || Content.Count == 0) return result;

            using (var regionNames = Content.KeysLocked)
            {
                foreach(var regionName in regionNames)
                {
                    var region = regionLookup(regionName) ?? Content[regionName];
                    if (region != null)
                        result.Add(region.WriteHtml(renderContext, dataContext));
                }
            }
            return result;
        }
    }
}
