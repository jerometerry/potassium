namespace Sodium.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using NUnit.Framework;

    [TestFixture]
    public class ContinuousBehaviorTester
    {
        [Test]
        public void TestDiscretize()
        {
            var timeB = new LocalTimeBehavior();
            var results = new List<DateTime>();

            var p = new QueryPredicateBehavior(() => results.Count >= 5);
            var evt = timeB.ToEvent(TimeSpan.FromMilliseconds(100), p);
            var s = evt.Subscribe(results.Add);
            evt.Start();

            while (!evt.Complete)
            {
                Thread.Sleep(0);
            }

            s.Dispose();
            evt.Dispose();
            Assert.AreEqual(5, results.Count);

            foreach (var t in results)
            {
                Console.WriteLine("{0:dd/MM/yy HH:mm:ss.fff}", t);
            }
        }
    }
}
