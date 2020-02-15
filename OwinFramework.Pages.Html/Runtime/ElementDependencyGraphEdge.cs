using OwinFramework.Interfaces.Utility;
using OwinFramework.Pages.Core.Interfaces;
using System;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class ElementDependencyGraphEdge: IDependencyGraphEdge
    {
        public string Key { get; set; }
        public bool Required { get; set; }

        public ElementDependencyGraphEdge(IElement dependentElement)
        {
            Key = dependentElement.FullyQualifiedName();
        }

        public bool Equals(IDependencyGraphEdge other)
        {
            if (ReferenceEquals(other, null)) return false;
            return string.Equals(Key, other.Key, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return Required ? Key : "~" + Key;
        }
    }
}
