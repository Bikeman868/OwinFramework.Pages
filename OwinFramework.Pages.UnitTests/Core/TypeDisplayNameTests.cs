using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OwinFramework.Pages.Core.Extensions;

namespace OwinFramework.Pages.UnitTests.Core
{
    [TestFixture]
    public class TypeDisplayNameTests
    {
        [Test]
        [TestCase(typeof(int), "Int32")]
        [TestCase(typeof(string), "String")]
        [TestCase(typeof(DateTime), "DateTime")]
        [TestCase(typeof(TypeDisplayNameTest), "OwinFramework.Pages.UnitTests.Core.TypeDisplayNameTest")]
        [TestCase(typeof(TypeDisplayNameTest.NestedClass), "OwinFramework.Pages.UnitTests.Core.TypeDisplayNameTest { NestedClass }")]
        [TestCase(typeof(List<int>), "List<Int32>")]
        [TestCase(typeof(IList<int>), "IList<Int32>")]
        [TestCase(typeof(IDictionary<String, TypeDisplayNameTest.NestedClass>), "IDictionary<String,OwinFramework.Pages.UnitTests.Core.TypeDisplayNameTest { NestedClass }>")]
        public void Should_format_value_types(Type t, string displayName)
        {
            Assert.AreEqual(displayName, t.DisplayName());
        }
    }

    internal class TypeDisplayNameTest
    {
        public class NestedClass
        {
             
        }
    }
}
