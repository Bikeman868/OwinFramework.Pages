using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// Implemented by elements that repet their contents for each
    /// data item in a list of bound data
    /// </summary>
    public interface IDataRepeater
    {
        /// <summary>
        /// The scope name used to resolve data references in the 
        /// repeated data
        /// </summary>
        string RepeatScope { get; set; }

        /// <summary>
        /// The type of data to repeat
        /// </summary>
        Type RepeatType { get; set; }

        /// <summary>
        /// The scope name to use when resolving the list from
        /// context
        /// </summary>
        string ListScope { get; set; }

        /// <summary>
        /// The type of list that the region will look for in the
        /// data context
        /// </summary>
        Type ListType { get; }
    }
}
