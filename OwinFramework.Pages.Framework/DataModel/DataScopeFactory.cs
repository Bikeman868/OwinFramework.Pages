using System;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Framework.DataModel
{
    internal class DataScopeFactory: IDataScopeFactory
    {
        public IDataScope Create(Type dataType, string scopeName = null)
        {
            return new DataScope
            {
                DataType = dataType,
                ScopeName = scopeName
            };
        }

        public IDataScope Create(string scopeName, Type dataType = null)
        {
            return new DataScope
            {
                DataType = dataType,
                ScopeName = scopeName
            };
        }
    }
}
