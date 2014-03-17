namespace Sodium.MemoryTests
{
    using NUnit.Framework;

    [TestFixture]
    public class MemoryTest5
    {
        [TestCase(1000000000)]
        public void Test(int iterations)
        {
            var sink = new EventSink<int>();
            var obserable = sink.Hold(0);
            var listener = obserable.SubscribeAndFire(tt => { });
            var i = 0;
            while (i < iterations)
            {
                sink.Fire(i);
                i++;
            }

            listener.Dispose();
        }
    }
}