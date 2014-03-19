namespace Sodium.MemoryTests
{
    using NUnit.Framework;

    [TestFixture]
    public class MemoryTest5
    {
        [TestCase(1000000000)]
        public void Test(int iterations)
        {
            var sink = new EventPublisher<int>();
            var obserable = sink.Hold(0);
            var listener = obserable.SubscribeValues(tt => { });
            var i = 0;
            while (i < iterations)
            {
                sink.Publish(i);
                i++;
            }

            listener.Dispose();
        }
    }
}