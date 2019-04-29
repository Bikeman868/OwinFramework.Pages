using Prius.Contracts.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        /// The name of a component to render on the page
        /// </summary>
        [Mapping("component")]
        public string ComponentName { get; set; }
    }
}
