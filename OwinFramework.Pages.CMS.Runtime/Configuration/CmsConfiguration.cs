﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwinFramework.Pages.CMS.Runtime.Configuration
{
    internal class CmsConfiguration
    {
        /// <summary>
        /// The Prius repository that contains the CMS data
        /// </summary>
        public string PriusRepositoryName { get; set; }

        /// <summary>
        /// The name of the website version to render by default. The editor
        /// can edit any version of the website, but each instance of the 
        /// runtime only displays one specific version.
        /// </summary>
        public string VersionName { get; set; }

        public CmsConfiguration()
        {
            PriusRepositoryName = "cms";
            VersionName = "1.0";
        }
    }
}
