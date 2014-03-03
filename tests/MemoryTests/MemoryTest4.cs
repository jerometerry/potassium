namespace Sodium.MemoryTests
{
    using NUnit.Framework;

    [TestFixture]
    public class MemoryTest4
    {
        [TestCase(1000000000)]
        public void Test(int iterations)
        {
            var evt = new EventSink<int>();
            var nestedEvent = new Event<int>();
            var behaviorOfEvents = evt.Map(x => nestedEvent).ToBehavior(nestedEvent);
            var observable = Behavior<int>.UnwrapEvent(behaviorOfEvents);
            var listen = observable.Listen(tt => { });
            var i = 0;
            while (i < iterations)
            {
                evt.Fire(i);
                i++;
            }

            listen.Dispose();
            observable.Dispose();
        }
    }
}