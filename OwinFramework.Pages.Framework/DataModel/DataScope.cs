using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Framework.DataModel
{
    public class DataScope: IDataScope
    {
        public Type DataType { get; set; }
        public string ScopeName { get; set; }

        private readonly List<IDataDependency> _dependencies = new List<IDataDependency>();

        public IList<IDataDependency> Dependencies { get { return _dependencies; } }

        public bool IsMatch(IDataDependency dependency)
        {
            if (string.IsNullOrEmpty(ScopeName))
                return DataType == dependency.DataType;

            if (DataType == null)
                return string.Equals(ScopeName, dependency.ScopeName, StringComparison.OrdinalIgnoreCase);

            return (DataType == dependency.DataType) &&
                string.Equals(ScopeName, dependency.ScopeName, StringComparison.OrdinalIgnoreCase);
        }

        public bool Add(IDataDependency dependency)
        {
            if (!IsMatch(dependency))
                return false;

            foreach (var existing in _dependencies)
            {
                if (string.IsNullOrEmpty(dependency.ScopeName) && string.IsNullOrEmpty(existing.ScopeName))
                {
                    if (existing.DataType == dependency.DataType)
                        return true;
                }
                if (existing.DataType == dependency.DataType &&
                    string.Equals(existing.ScopeName, dependency.ScopeName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            _dependencies.Add(dependency);

            return true;
        }
    }
}
