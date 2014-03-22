namespace JT.Rx.Net.Tests
{
    using System;
    using JT.Rx.Net.Extensions;
    using JT.Rx.Net.Monads;
    using NUnit.Framework;

    [TestFixture]
    public class MonadTester
    {
        private const double TwoPi = 2.0 * Math.PI;
        private const double PiBy2 = Math.PI / 2.0;

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
            var u = new UnaryMonad<double, double>(Math.Sin, TwoPi.ToIdentity());
            Console.WriteLine(u.Value);
            Assert.AreEqual(0.0,u.Value, 1e-5);
            
            u = new UnaryMonad<double, double>(Math.Sin, PiBy2.ToIdentity());
            Assert.AreEqual(1.0, u.Value, 1e-5);
        }
    }
}
