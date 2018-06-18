using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;
using OwinFramework.Pages.Html.Runtime.Internal;

namespace OwinFramework.Pages.Html.Builders
{
    // TODO: Data binding
    // TODO: Repeating content on binding to a list
    // TODO: Render styles to dynamic assets
    // TODO: Implement AssetDeployment

    internal class RegionBuilder: IRegionBuilder
    {
        private readonly INameManager _nameManager;
        private readonly IHtmlHelper _htmlHelper;

        public RegionBuilder(
            INameManager nameManager,
            IHtmlHelper htmlHelper)
        {
            _nameManager = nameManager;
            _htmlHelper = htmlHelper;
        }

        IRegionDefinition IRegionBuilder.Region()
        {
            return new RegionDefinition(_nameManager, _htmlHelper);
        }
    }
}
