namespace JT.Rx.Net.MemoryTests
{
    using JT.Rx.Net.Core;
    using JT.Rx.Net.Continuous;
    using JT.Rx.Net.Discrete;
    using NUnit.Framework;

    [TestFixture]
    public class MemoryTest5
    {
        [TestCase(1000000000)]
        public void Test(int iterations)
        {
            var sink = new EventPublisher<int>();
            var obserable = sink.Hold(0);
            var values = obserable.Values();
            var listener = values.Subscribe(tt => { });
            var i = 0;
            while (i < iterations)
            {
                sink.Publish(i);
                i++;
            }

            listener.Dispose();
            values.Dispose();
        }
    }
}