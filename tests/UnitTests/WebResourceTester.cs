namespace Potassium.Tests
{
    using System;
    using Potassium.Providers;
    
    using NUnit.Framework;

    [TestFixture]
    public class WebResourceTester
    {
        [Test]
        public void LoadGoogle()
        {
            var provider = new WebResource("http://google.com");
            var source = provider.Value;
            Console.WriteLine("Google.com Source: {0}", source);
			Assert.IsNotNull(source);
        }

        [Test]
        public void LoadJsonTestIp()
        {
            var provider = new WebResource("http://ip.jsontest.com/");
            var json = provider.Value;
            Console.WriteLine("JSON: {0}", json);
            Assert.IsNotNull(json);
        }
    }
}
