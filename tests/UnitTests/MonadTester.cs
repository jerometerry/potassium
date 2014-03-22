namespace JT.Rx.Net.Tests
{
    using System;
    using JT.Rx.Net.Core;
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

        public static Tuple<short, int> NextShort(int state)
        {
            const int multiplier = 214013;
            const int increment = 2531011;
            const int modulus = int.MaxValue;
            var newState = multiplier * state + increment;
            var rand = (short)((newState & modulus) >> 16);
            return Tuple.Create(rand, newState);
        }

        public State<int, short> GetRandom()
        {
            var s = State.Get<int>();
            return s.Bind(s0 =>
            {
                var rand = NextShort(s0);
                var setState = State.Set(rand.Item2);
                return setState.Bind(_ => new State<int, short>(a => Tuple.Create(rand.Item2, rand.Item1)));
            });
        }

        [Test]
        public void TestRandom()
        {
            var state = GetRandom().Bind(
                a => GetRandom().Bind(
                    b => GetRandom().Bind(
                        c => (a + b + c)
                            .ToState<int, int>())));

            var result = state.Computation(0);
            Console.WriteLine(result.Item2);
        }
    }
}
