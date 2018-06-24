using System;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Framework.DataModel
{
    internal class DataDependency: IDataDependency
    {
        public Type DataType { get; set; }
        public string ScopeName { get; set; }
    }
}
