using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using Moq.Modules;
using OwinFramework.Pages.Core.Interfaces.Collections;

namespace OwinFramework.Pages.Mocks.Collections
{
    public class MockStringBuilderFactory : MockImplementationProvider<IStringBuilderFactory>
    {
        protected override void SetupMock(IMockProducer mockProducer, Mock<IStringBuilderFactory> mock)
        {
            mock.Setup(f => f.Create()).Returns(() => new MockStringBuilder());
            mock.Setup(f => f.Create(It.IsAny<long>())).Returns<long>(c => new MockStringBuilder());
            mock.Setup(f => f.Create(It.IsAny<string>())).Returns<string>(s => new MockStringBuilder(s));
        }

        private class MockStringBuilder : IStringBuilder
        {
            private StringBuilder _stringBuilder;

            public MockStringBuilder()
            {
                _stringBuilder = new StringBuilder();
            }

            public MockStringBuilder(string data)
            {
                _stringBuilder = new StringBuilder(data);
            }

            public IStringBuilder Append(int value)
            {
                _stringBuilder.Append(value);
                return this;
            }

            public IStringBuilder Append(char text)
            {
                _stringBuilder.Append(text);
                return this;
            }

            public IStringBuilder Append(string text)
            {
                _stringBuilder.Append(text);
                return this;
            }

            public IStringBuilder Append<T>(T value)
            {
                _stringBuilder.Append(value);
                return this;
            }

            public IArray<char> ToArray(bool extractBuffer = false)
            {
                throw new NotImplementedException();
            }

            public IStringBuilder AppendFormat(string format, params object[] arguments)
            {
                _stringBuilder.AppendFormat(format, arguments);
                return this;
            }

            public IStringBuilder AppendLine(string line)
            {
                _stringBuilder.AppendLine(line);
                return this;
            }

            public void Clear()
            {
                _stringBuilder.Clear();
            }

            public long Length
            {
                get
                {
                    return _stringBuilder.Length;
                }
                set
                {
                    _stringBuilder.Length = (int)value;
                }
            }

            public override string ToString()
            {
                return _stringBuilder.ToString();
            }

            public void Dispose()
            {
            }

            public IEnumerator<char> GetEnumerator()
            {
                return _stringBuilder.ToString().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _stringBuilder.ToString().GetEnumerator();
            }

            public char[] GetArray()
            {
                return _stringBuilder.ToString().ToArray();
            }

            public long Size
            {
                get { return Length; }
            }

            public char this[long index]
            {
                get
                {
                    return _stringBuilder.ToString()[(int)index];
                }
                set
                {
                    var oldString = _stringBuilder.ToString();
                    var left = index > 0 ? oldString.Substring(0, (int)index) : String.Empty;
                    var right = index < oldString.Length - 1 ? oldString.Substring((int)index + 1) : String.Empty;
                    _stringBuilder = new StringBuilder(left + value + right);
                }
            }

            public bool IsDisposed
            {
                get { return false; }
            }

            public bool IsDisposing
            {
                get { return false; }
            }

            public bool IsReusable
            {
                get { return false; }
            }
        }
    }
}
