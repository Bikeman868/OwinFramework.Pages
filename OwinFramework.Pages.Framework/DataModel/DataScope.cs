using System;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Framework.DataModel
{
    public class DataScope: IDataScope
    {
        public Type DataType { get; set; }
        public string ScopeName { get; set; }
        public bool IsProvidedByElement { get; set; }

        public bool IsMatch(IDataDependency dependency)
        {
            if (string.IsNullOrEmpty(ScopeName))
                return DataType == dependency.DataType;

            if (DataType == null)
                return string.Equals(ScopeName, dependency.ScopeName, StringComparison.OrdinalIgnoreCase);

            return (DataType == dependency.DataType) &&
                string.Equals(ScopeName, dependency.ScopeName, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return "Data scope " + (string.IsNullOrEmpty(ScopeName) ? "" : "'" + ScopeName + "' ") + DataType.DisplayName();
        }
    }
}
