using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.Pages.CMS.Editor.Configuration
{
    public class Permissions
    {
        /// <summary>
        /// Allow users to view the CMS Editor
        /// </summary>
        public const string View = "cms:view";

        // These permissions have the name of the element as the asset name
        // so that you can grant editing permissions on individual elements

        public const string EditEnvironment = "cms:edit/environment";
        public const string EditComponent = "cms:edit/component";
        public const string EditDataScope = "cms:edit/datascope";
        public const string EditDataType = "cms:edit/datatype";
        public const string EditLayout = "cms:edit/layout";
        public const string EditModule = "cms:edit/module";
        public const string EditPage = "cms:edit/page";
        public const string EditRegion = "cms:edit/region";

        // These permissions have the environment name as the asset name
        // so that you can grant access to specific environments where the
        // website runs

        public const string ChangeEnvironmentVersion = "cms:version/environment";
        public const string ChangeComponentVersion = "cms:version/component";
        public const string ChangeDataTypeVersion = "cms:version/datatype";
        public const string ChangeLayoutVersion = "cms:version/layout";
        public const string ChangePageVersion = "cms:version/page";
        public const string ChangeRegionVersion = "cms:version/region";
    }
}
