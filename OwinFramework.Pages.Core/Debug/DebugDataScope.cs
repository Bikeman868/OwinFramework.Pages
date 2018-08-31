using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.Pages.Core.Extensions;

namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// This is used to communicate debugging information about a scope
    /// </summary>
    public class DebugDataScope : DebugInfo
    {
        /// <summary>
        /// The type of data in scope
        /// </summary>
        public Type DataType { get; set; }

        /// <summary>
        /// An optional name used to separate multiple data of the same type
        /// </summary>
        public string ScopeName { get; set; }

        /// <summary>
        /// Override ToString to return default description
        /// </summary>
        public override string ToString()
        {
            var result = DataType.DisplayName(TypeExtensions.NamespaceOption.Ending);

            if (!string.IsNullOrEmpty(ScopeName))
                result = "'" + ScopeName + "' " + result;

            return result;
        }
    }
}
