namespace Sodium.Tests
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class HttpBehaviorTester
    {
        [Test]
        public void LoadGoogle()
        {
            var behavior = new HttpBehavior("http://google.com");
            var source = behavior.Value;
            Console.WriteLine("Google.com Source: {0}", source);
            Assert.IsNotNullOrEmpty(source);
        }
        [Test]
        public void LoadJsonTestIp()
        {
            var behavior = new HttpBehavior("http://ip.jsontest.com/");
            var json = behavior.Value;
            Console.WriteLine("JSON: {0}", json);
            Assert.IsNotNullOrEmpty(json);
        }
    }
}
