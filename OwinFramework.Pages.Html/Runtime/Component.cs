using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;

namespace OwinFramework.Pages.Html.Runtime
{
    /// <summary>
    /// Base implementation of IComponent. Inheriting from this olass will insulate you
    /// from any future additions to the IComponent interface
    /// </summary>
    public class Component : Element, IComponent
    {
        public override ElementType ElementType { get { return ElementType.Component; } }

        DebugElement IElement.GetDebugInfo() { return GetDebugInfo(); }

        public Component(IComponentDependenciesFactory componentDependenciesFactory)
        {
            // DO NOT change the method signature of this constructor as
            // this would break all components in all applications that use
            // this framework!!
        }

        public DebugComponent GetDebugInfo()
        {
            var debugInfo = new DebugComponent
            {
            };
            PopulateDebugInfo(debugInfo);
            return debugInfo;
        }
    }
}
