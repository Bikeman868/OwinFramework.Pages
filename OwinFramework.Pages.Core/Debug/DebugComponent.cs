﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// Debug information about a layout
    /// </summary>
    public class DebugComponent: DebugElement
    {
        /// <summary>
        /// Default public constructor
        /// </summary>
        public DebugComponent()
        {
            Type = "Component";
        }
    }
}
