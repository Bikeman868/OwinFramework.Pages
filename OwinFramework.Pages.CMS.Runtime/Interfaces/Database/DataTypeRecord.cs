using System;
using Prius.Contracts.Attributes;

namespace OwinFramework.Pages.CMS.Runtime.Interfaces.Database
{
    /// <summary>
    /// Used to expose a C# class to the CMS that can be selected
    /// as the type of data to repeat in a region
    /// </summary>
    public class DataTypeRecord: ElementRecordBase
    {
        public DataTypeRecord()
        {
            RecordType = "DataType";
        }
    }
}
