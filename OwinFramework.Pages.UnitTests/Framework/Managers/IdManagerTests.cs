using System;
using System.Collections.Generic;
using System.Threading;
using Moq.Modules;
using NUnit.Framework;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Framework.Managers;

namespace OwinFramework.Pages.UnitTests.Framework.Managers
{
    [TestFixture]
    public class IdManagerTests: TestBase
    {
        [SetUp]
        public void Setup()
        {
            Reset();
        }

        [Test]
        public void Should_generate_unique_ids()
        {
            var idManager = new IdManager() as IIdManager;
            var hashSet = new HashSet<int>();
            var failed = false;

            Action task = () =>
                {
                    var list = new List<int>();
                    for (var i = 0; i < 10000; i++)
                    {
                        list.Add(idManager.GetUniqueId());
                        Thread.Sleep(0);
                    }
                    foreach(var id in list)
                    {
                        lock(hashSet)
                        {
                            if (!hashSet.Add(id))
                                failed = true;
                        }
                    }
                };

            var threads = new List<Thread>();

            for (var j = 0; j < 100; j++)
                threads.Add(new Thread(new ThreadStart(task)));

            foreach (var thread in threads)
                thread.Start();

            foreach (var thread in threads) 
                thread.Join();

            Assert.IsFalse(failed);
            Assert.AreEqual(1000000, hashSet.Count);
        }
    }
}
