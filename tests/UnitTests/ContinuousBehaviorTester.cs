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

        [Test]
        public void TestRandomSin()
        {
            var rd = new RandomDoubleBehavior(2 * Math.PI);
            var sb = new Map<double, double>(Math.Sin);
            var s = rd.Apply(sb);
            for (int i = 0; i < 5; i++) 
            {
                Console.WriteLine(s.Value);
            }
        }

        [Test]
        public void TestSin()
        {
            var b = new Behavior<double>(2 * Math.PI);
            var sb = new Map<double, double>(Math.Sin);
            var s = b.Apply(sb);
            Console.WriteLine(s.Value);
            Assert.AreEqual(0.0, s.Value, 1e-5);

            b.SetValue(Math.PI / 2.0);
            Assert.AreEqual(1.0, s.Value, 1e-5);
        }
    }
}
