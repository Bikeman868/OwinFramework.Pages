using System;
using Newtonsoft.Json;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.LiveUpdate
{
    /// <summary>
    /// This is used to identify a list of children beloning to a 
    /// parent record
    /// </summary>
    public class RecordChildrenReference: RecordReference
    {
        /// <summary>
        /// The type of children type. Can be 'Zone', 'Route' etc
        /// </summary>
        [JsonProperty("childRecordType")]
        public string ChildRecordType { get; set; }
    }
}