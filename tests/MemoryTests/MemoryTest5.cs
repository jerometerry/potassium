namespace Potassium.MemoryTests
{
    using Potassium.Core;
    using NUnit.Framework;

    [TestFixture]
    public class MemoryTest5
    {
        [TestCase(1000000000)]
        public void Test(int iterations)
        {
            var sink = new FirableEvent<int>();
            var obserable = sink.Hold(0);
            var values = obserable.Values();
            var listener = values.Subscribe(tt => { });
            var i = 0;
            while (i < iterations)
            {
                sink.Fire(i);
                i++;
            }

            listener.Dispose();
            values.Dispose();
        }
    }
}