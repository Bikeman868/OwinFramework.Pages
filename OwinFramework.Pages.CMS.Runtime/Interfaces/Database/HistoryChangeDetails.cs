using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    public class HistoryChangeDetails
    {
        /// <summary>
        /// Can be 'Created', 'Deleted', 'Modified', 'ChildAdded', 'ChildRemoved'
        /// </summary>
        [JsonProperty("changeType")]
        public string ChangeType { get; set; }

        /// <summary>
        /// If this is a modification then this contains the name of the field that was modified
        /// </summary>
        [JsonProperty("field")]
        public string FieldName { get; set; }

        /// <summary>
        /// If this is a modification then this contains the original value
        /// </summary>
        [JsonProperty("oldValue")]
        public string OldValue { get; set; }

        /// <summary>
        /// If this is a modification then this contains the new value
        /// </summary>
        [JsonProperty("newvalue")]
        public string NewValue { get; set; }

        /// <summary>
        /// If this is an addition or removal then this contains the type of child that was added or removed
        /// </summary>
        [JsonProperty("childType")]
        public string ChildType { get; set; }

        /// <summary>
        /// If this is an addition or removal then this contains the id of child that was added or removed
        /// </summary>
        [JsonProperty("childId")]
        public long? ChildId { get; set; }
    }
}
