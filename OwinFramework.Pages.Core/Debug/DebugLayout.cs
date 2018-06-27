﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// Debug information about a layout
    /// </summary>
    public class DebugLayout : DebugElement
    {
        /// <summary>
        /// The layout's regions
        /// </summary>
        public List<DebugLayoutRegion> Regions { get; set; }

        /// <summary>
        /// If this is a clone then this contains debug info
        /// for the layout that was cloned
        /// </summary>
        public DebugLayout ClonedFrom { get; set; }

        /// <summary>
        /// Default public constructor
        /// </summary>
        public DebugLayout()
        {
            Type = "Layout";
        }
    }
}