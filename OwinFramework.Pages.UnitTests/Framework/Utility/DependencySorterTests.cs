using System;
using System.Collections.Generic;
using System.Linq;
using Moq.Modules;
using NUnit.Framework;
using OwinFramework.Pages.Framework.Utility;

namespace OwinFramework.Pages.UnitTests.Framework.Utility
{
    [TestFixture]
    public class DependencySorterTests: TestBase
    {
        DependencyListSorter<TestType> _dependencyListSorter;
            
        [SetUp]
        public void Setup()
        {
            Reset();
            _dependencyListSorter = new DependencyListSorter<TestType>();
        }

        [Test]
        public void Test_type_works()
        {
            var testTypes = new []
            {
                new TestType(0),
                new TestType(1, 0),
                new TestType(2, 1, 0),
                new TestType(3, 2)
            };

            Assert.IsFalse(TestType.IsDependentOn(testTypes[0], testTypes[1]));
            Assert.IsFalse(TestType.IsDependentOn(testTypes[0], testTypes[2]));
            Assert.IsFalse(TestType.IsDependentOn(testTypes[1], testTypes[2]));
            Assert.IsFalse(TestType.IsDependentOn(testTypes[2], testTypes[3]));
            Assert.IsFalse(TestType.IsDependentOn(testTypes[3], testTypes[1]));

            Assert.IsTrue(TestType.IsDependentOn(testTypes[1], testTypes[0]));
            Assert.IsTrue(TestType.IsDependentOn(testTypes[2], testTypes[0]));
            Assert.IsTrue(TestType.IsDependentOn(testTypes[2], testTypes[1]));
            Assert.IsTrue(TestType.IsDependentOn(testTypes[3], testTypes[2]));
        }

        [Test]
        [TestCase("In order", new[] { 0, 1, 2, 3, 4, 5 })]
        [TestCase("Reverse order", new[] { 5, 4, 3, 2, 1, 0 })]
        [TestCase("Last first", new[] { 5, 0, 1, 2, 3, 4 })]
        [TestCase("First last", new[] { 0, 5, 4, 3, 2, 1 })]
        [TestCase("Random", new[] { 3, 5, 1, 0, 4, 2 })]
        public void Should_sort_full_dependencies(string description, int[] order)
        {
            var originalList = new List<TestType> 
            { 
                new TestType(0),
                new TestType(1, 0),
                new TestType(2, 0, 1),
                new TestType(3, 2),
                new TestType(4, 1, 3),
                new TestType(5, 4)
            };

            var orderedList = new List<TestType>(originalList);
            for (var i = 0; i < order.Length; i++)
                orderedList[order[i]] = originalList[i];

            var sortedList = _dependencyListSorter.Sort(orderedList, TestType.IsDependentOn);

            for (var i = 0; i < sortedList.Count; i++)
                Assert.AreEqual(i, sortedList[i].Id, description);
        }

        [Test]
        [TestCase("In order", new[] { 0, 1, 2, 3, 4, 5 })]
        [TestCase("Reverse order", new[] { 5, 4, 3, 2, 1, 0 })]
        [TestCase("Last first", new[] { 5, 0, 1, 2, 3, 4 })]
        [TestCase("First last", new[] { 0, 5, 4, 3, 2, 1 })]
        [TestCase("Random", new[] { 3, 5, 1, 0, 4, 2 })]
        public void Should_sort_partial_dependencies(string description, int[] order)
        {
            var originalList = new List<TestType> 
            { 
                new TestType(0),
                new TestType(1),
                new TestType(2, 3),
                new TestType(3, 1),
                new TestType(4, 2),
                new TestType(5, 1)
            };

            var orderedList = new List<TestType>(originalList);
            for (var i = 0; i < order.Length; i++)
                orderedList[order[i]] = originalList[i];

            var sortedList = _dependencyListSorter.Sort(orderedList, TestType.IsDependentOn);

            Func<int, int> pos = id => sortedList.FindIndex(i => i.Id == id);

            Assert.IsTrue(pos(2) > pos(3));
            Assert.IsTrue(pos(3) > pos(1));
            Assert.IsTrue(pos(4) > pos(2));
            Assert.IsTrue(pos(5) > pos(1));
        }

        [Test]
        [TestCase("In order", new[] { 0, 1, 2, 3, 4, 5 })]
        [TestCase("Reverse order", new[] { 5, 4, 3, 2, 1, 0 })]
        [TestCase("Last first", new[] { 5, 0, 1, 2, 3, 4 })]
        [TestCase("First last", new[] { 0, 5, 4, 3, 2, 1 })]
        [TestCase("Random", new[] { 3, 5, 1, 0, 4, 2 })]
        public void Should_ignore_out_of_scope_dependencies(string description, int[] order)
        {
            var originalList = new List<TestType> 
            { 
                new TestType(0),
                new TestType(1, 7),
                new TestType(2, 3),
                new TestType(3, 1, 8),
                new TestType(4, 2),
                new TestType(5, 1)
            };

            var orderedList = new List<TestType>(originalList);
            for (var i = 0; i < order.Length; i++)
                orderedList[order[i]] = originalList[i];

            var sortedList = _dependencyListSorter.Sort(orderedList, TestType.IsDependentOn);

            Func<int, int> pos = id => sortedList.FindIndex(i => i.Id == id);

            Assert.IsTrue(pos(2) > pos(3));
            Assert.IsTrue(pos(3) > pos(1));
            Assert.IsTrue(pos(4) > pos(2));
            Assert.IsTrue(pos(5) > pos(1));
        }

        private class TestType
        {
            public readonly int Id;
            private readonly List<int> _dependsOn;

            public TestType(int id, params int[] dependsOn)
            {
                Id = id;
                _dependsOn = dependsOn == null ? new List<int>() : dependsOn.ToList();
            }

            public override string ToString()
            {
                return Id + " [" + string.Join(",", _dependsOn) + "]";
            }

            public static bool IsDependentOn(TestType t1, TestType t2)
            {
                return t1._dependsOn.Any(i => i == t2.Id);
            }
        }
    }
}
