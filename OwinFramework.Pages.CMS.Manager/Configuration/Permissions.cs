namespace OwinFramework.Pages.CMS.Manager.Configuration
{
    public class Permissions
    {
        /// <summary>
        /// Allow users to view the CMS Manager
        /// </summary>
        public const string View = "cms:view";

        // These permissions have the name of the element as the asset name
        // so that you can grant editing permissions on individual elements

        public const string EditEnvironment = "cms:edit/environment";
        public const string EditWebsiteVersion = "cms:edit/version";
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

        public const string ChangeElementVersion = "cms:version/element";
        public const string ChangeEnvironmentVersion = "cms:version/environment";

        // These permissions relate to A/B testing

        public const string EditSegmentTest = "cms:edit/segmentation/test";
        public const string EditSegmentScenario = "cms:edit/segmentation/scenario";
    }
}
