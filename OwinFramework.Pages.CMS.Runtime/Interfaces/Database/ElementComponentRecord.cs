using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    public class ElementComponentRecord
    {
        /// <summary>
        /// The page version that this component should be rendered onto
        /// </summary>
        [Mapping("elementVersionId")]
        public long ElementVersionId { get; set; }

        /// <summary>
        /// The name of a component to render on the page. This can be a component
        /// defined in code or the name of a component version from CMS
        /// </summary>
        [Mapping("component")]
        public string ComponentName { get; set; }
    }
}
