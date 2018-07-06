using System;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Framework.DataModel
{
    internal class DataDependencyFactory: IDataDependencyFactory
    {
        public IDataDependency Create(Type type, string scopeName = null)
        {
            return new DataDependency
            {
                DataType = type,
                ScopeName = scopeName
            };
        }

        public IDataDependency Create<T>(string scopeName = null)
        {
            return new DataDependency
            {
                DataType = typeof(T),
                ScopeName = scopeName
            };
        }
    }
}
