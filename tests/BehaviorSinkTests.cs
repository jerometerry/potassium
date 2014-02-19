namespace sodium.tests
{
    using NUnit.Framework;
    using System;

    [TestFixture]
    public class BehaviorSinkTests : SodiumTestCase
    {
        [Test]
        public void Constructor_PassIntValue_ExpectValueSet()
        {
            var sink = new BehaviorSink<int>(123);
            Assert.AreEqual(123, sink.NewValue());
        }

        [Test]
        public void Constructor_PassNullableIntValue_ExpectNullValue()
        {
            var sink = new BehaviorSink<int?>(null);
            Assert.AreEqual(null, sink.NewValue());
        }

        [Test]
        public void Send_PassIntValue_ExpectValueUpdated()
        {
            var sink = new BehaviorSink<int>(0);
            sink.Send(1);
            Assert.AreEqual(1, sink.NewValue());
        }

        [Test]
        public void Send_PassIntValueTwice_ExpectSecondValue()
        {
            var sink = new BehaviorSink<int>(0);
            sink.Send(1);
            sink.Send(2);
            Assert.AreEqual(2, sink.NewValue());
        }
    }
}
