namespace Sodium.MemoryTests
{
    using NUnit.Framework;

    [TestFixture]
    public class MemoryTest3
    {
        [TestCase(1000000000)]
        public void Test(int iterations)
        {
            var evt = new Event<int>();
            var et = new Event<int>();
            var behavior = et.ToBehavior(0);
            var eventOfBehaviors = evt.Map(x => behavior).ToBehavior(behavior);
            var observable = Behavior<int>.Unwrap(eventOfBehaviors);
            var l = observable.GetValueStream().Listen(tt => { });
            var i = 0;
            while (i < iterations)
            {
                evt.Fire(i);
                i++;
            }

            l.Dispose();
        }
    }
}