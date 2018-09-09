using System;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.DataModel
{
    internal class DataDependency: IDataDependency, IDebuggable
    {
        public Type DataType { get; set; }
        public string ScopeName { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(ScopeName)
                ? DataType.DisplayName(TypeExtensions.NamespaceOption.Ending)
                : "'" + ScopeName + "' " + DataType.DisplayName(TypeExtensions.NamespaceOption.Ending);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            
            var other = obj as IDataDependency;
            if (other == null) return false;

            if (DataType != other.DataType) return false;

            if (string.IsNullOrEmpty(ScopeName))
                return string.IsNullOrEmpty(other.ScopeName);

            return string.Equals(ScopeName, other.ScopeName, StringComparison.InvariantCultureIgnoreCase);
        }

        bool IEquatable<IDataDependency>.Equals(IDataDependency other)
        {
            return Equals(other);
        }

        T IDebuggable.GetDebugInfo<T>(int parentDepth, int childDepth)
        {
            return new DebugDataScope
            {
                Instance = this,
                Type = "Data dependency",
                DataType = DataType,
                ScopeName = ScopeName
            } as T;
        }
    }
}
