namespace JT.Rx.Net.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    
    using NUnit.Framework;

    [TestFixture]
    public class WebResourceTester
    {
        [Test]
        public void LoadGoogle()
        {
            var behavior = new WebResource("http://google.com");
            var source = behavior.Value;
            Console.WriteLine("Google.com Source: {0}", source);
            Assert.IsNotNullOrEmpty(source);
        }

        [Test]
        public void LoadJsonTestIp()
        {
            var behavior = new WebResource("http://ip.jsontest.com/");
            var json = behavior.Value;
            Console.WriteLine("JSON: {0}", json);
            Assert.IsNotNullOrEmpty(json);
        }
    }
}
