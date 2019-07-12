﻿using System;
using Newtonsoft.Json;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// Used to expose a C# class to the CMS that can be selected
    /// as the type of data to repeat in a region
    /// </summary>
    public class DataScopeRecord: RecordBase
    {
        public DataScopeRecord()
        {
            RecordType = "DataScope";
        }

        /// <summary>
        /// The unique ID of this data scope
        /// </summary>
        [Mapping("dataScopeId")]
        [JsonProperty("dataScopeId")]
        public long DataScopeId { get; set; }

        /// <summary>
        /// Optional data type specifier. If this is missing then the Name
        /// property is assumed to be the name of a data provider
        /// </summary>
        [Mapping("dataTypeId")]
        [JsonProperty("dataTypeId")]
        public long? DataTypeId { get; set; }
    }
}
