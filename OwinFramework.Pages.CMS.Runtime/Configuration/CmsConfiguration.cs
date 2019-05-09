﻿namespace OwinFramework.Pages.CMS.Runtime.Configuration
{
    internal class CmsConfiguration
    {
        /// <summary>
        /// The Prius repository that contains the CMS data
        /// </summary>
        public string CmsRepositoryName { get; set; }

        /// <summary>
        /// The Prius repository that contains the CMS data
        /// </summary>
        public string LiveUpdateRepositoryName { get; set; }

        /// <summary>
        /// The name of the website version to render by default. The editor
        /// can edit any version of the website, but each instance of the 
        /// runtime only displays one specific version.
        /// </summary>
        public string WebsiteVersionName { get; set; }

        public CmsConfiguration()
        {
            CmsRepositoryName = "cms";
            LiveUpdateRepositoryName = "live-update";
            WebsiteVersionName = "1.0";
        }
    }
}
