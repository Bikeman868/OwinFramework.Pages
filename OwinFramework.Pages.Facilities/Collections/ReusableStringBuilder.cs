using System;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Facilities.Collections
{
    /// <summary>
    /// An implementation of IStringBuilder that is re-usable
    /// </summary>
    internal class ReusableStringBuilder : ReusableCollection<char>, IStringBuilder
    {
        public ReusableStringBuilder(IArrayFactory arrayFactory)
            : base(arrayFactory)
        {
        }

        public new IStringBuilder Initialize(Action<IReusable> disposeAction, Int64 capacity)
        {
            base.Initialize(disposeAction, capacity);
            return this;
        }

        public override string ToString()
        {
            if (ReusableArray == null || Length < 1) return String.Empty;
            return new String(ReusableArray.GetArray(), 0, (int)Length);
        }

        public IArray<char> ToArray(bool extractBuffer)
        {
            if (extractBuffer) return ExtractBuffer();
            return ReusableArray;
        }

        public IStringBuilder Append(string text)
        {
            if (string.IsNullOrEmpty(text)) return this;
            var startIndex = Length;
            Length = Length + text.Length;
            text.CopyTo(0, ReusableArray.GetArray(), (int)startIndex, text.Length);
            return this;
        }

        public IStringBuilder Append(char text)
        {
            var index = Length;
            Length = Length + 1;
            ReusableArray[index] = text;
            return this;
        }

        public IStringBuilder Append(int value)
        {
            Append(value.ToString());
            return this;
        }

        public IStringBuilder AppendFormat(string format, params object[] arguments)
        {
            Append(string.Format(format, arguments));
            return this;
        }

        public IStringBuilder AppendLine(string line)
        {
            Append(line);
            Append(Environment.NewLine);
            return this;
        }

        public IStringBuilder Append<T>(T value)
        {
            Append(value.ToString());
            return this;
        }
    }
}
