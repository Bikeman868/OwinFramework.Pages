using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Elements
{
    /// <summary>
    /// This is an instance of a component that is placed on a page
    /// </summary>
    internal class PageComponent : PageElement<IComponent>, IComponent
    {
        public override ElementType ElementType { get { return ElementType.Component; } }

        private readonly IComponentDependenciesFactory _dependenciesFactory;

        public PageComponent(
            IComponentDependenciesFactory dependencies,
            IComponent parent)
            : base(parent)
        {
            _dependenciesFactory = dependencies;
        }

        protected override DebugInfo PopulateDebugInfo(DebugInfo debugInfo)
        {
            var parentDebugInfo = Parent == null ? null : (DebugRegion)Parent.GetDebugInfo();
            var debugComponent = debugInfo as DebugComponent ?? new DebugComponent();

            return base.PopulateDebugInfo(debugComponent);
        }

        public override IWriteResult WriteHtml(IRenderContext context, bool includeChildren)
        {
            return Parent.WriteHtml(context, includeChildren);
        }

        public override IWriteResult WriteStaticCss(ICssWriter writer)
        {
            return Parent.WriteStaticCss(writer);
        }

        public override IWriteResult WriteStaticJavascript(IJavascriptWriter writer)
        {
            return Parent.WriteStaticJavascript(writer);
        }
    }
}
