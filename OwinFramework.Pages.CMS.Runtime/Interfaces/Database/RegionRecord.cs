﻿namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// A POCO that defines the database fields of region records
    /// </summary>
    public class RegionRecord: ElementRecordBase
    {
        public const string RecordTypeName = "Region";

        public RegionRecord()
        {
            RecordType = RecordTypeName;
        }
    }
}
