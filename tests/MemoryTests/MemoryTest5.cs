namespace Sodium.MemoryTests
{
    using NUnit.Framework;

    [TestFixture]
    public class MemoryTest5
    {
        [Test]
        public void Test()
        {
            var sink = new EventSink<int>();
            var o = sink.Hold(0);
            var l = o.Value().Listen(tt => { });
            var i = 0;
            while (i < 1000000000)
            {
                sink.Send(i);
                i++;
            }

            l.Stop();
        }
    }
}