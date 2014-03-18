namespace Sodium.MemoryTests
{
    using NUnit.Framework;

    [TestFixture]
    public class MemoryTest3
    {
        [TestCase(1000000000)]
        public void Test(int iterations)
        {
            var evt = new Sink<int>();
            var et = new Event<int>();
            var behavior = et.Hold(0);
            var eventOfBehaviors = evt.Map(x => behavior).Hold(behavior);
            var observable = Behavior<int>.SwitchB(eventOfBehaviors);
            var l = observable.SubscribeValues(tt => { });
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