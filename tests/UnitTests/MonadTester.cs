namespace JT.Rx.Net.Tests
{
    using System;
    using JT.Rx.Net.Monads;
    using NUnit.Framework;

    [TestFixture]
    public class MonadTester
    {
        private const double TwoPi = 2 * Math.PI;
        
        [Test]
        public void TestRandomSin()
        {
            var rd = new RandomDouble(TwoPi);
            var sb = new Map<double, double>(Math.Sin);
            var s = rd.Bind(sb);
            for (int i = 0; i < 5; i++) 
            {
                Console.WriteLine(s.Value);
            }
        }

        [Test]
        public void TestSin()
        {
            var b = TwoPi.ToIdentity();
            var sb = new Map<double, double>(Math.Sin);
            var s = b.Bind(sb);
            Console.WriteLine(s.Value);
            Assert.AreEqual(0.0, s.Value, 1e-5);

            b.SetValue(Math.PI / 2.0);
            Assert.AreEqual(1.0, s.Value, 1e-5);
        }
    }
}
