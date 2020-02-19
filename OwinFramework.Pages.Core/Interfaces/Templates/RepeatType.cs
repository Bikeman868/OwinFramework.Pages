using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.Pages.Core.Interfaces.Templates
{
    /// <summary>
    /// For templates with repeating sections, defines how the repetition is handled
    /// </summary>
    public enum RepeatType
    {
        /// <summary>
        /// With this type of repeating the repeated section is repeated for
        /// every item on a list. A data provider for IList{T} will be identified
        /// and the list that it provides will be enumerated
        /// </summary>
        RepeatList,

        /// <summary>
        /// With this type of repeater the repeated section is only repeated once
        /// and only if the specified property is true. In this case the data provider
        /// foe the type will be used to add data to the data context then a property
        /// of that data is queried to see if it is truthy to determine if the 
        /// content should be rendered or not
        /// </summary>
        IfTrue,

        /// <summary>
        /// This is the inverse of IfTrue where the content is only rendered if the
        /// property is not truthy
        /// </summary>
        IfNotTrue
    }
}
