namespace JT.Rx.Net.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using JT.Rx.Net.Continuous;
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
        public void PollJsonTestIp()
        {
            var results = new List<string>();
            var p = new QueryPredicate(() => results.Count >= 3);
            var behavior = new WebResource("http://ip.jsontest.com/");
            var evt = behavior.ToEvent(TimeSpan.FromMilliseconds(250), p);
            var hold = evt.Hold("N/A");
            var values = hold.Values();
            var s = values.Subscribe(results.Add);
            evt.Start();

            while (!evt.Complete)
            {
                Thread.Sleep(0);
            }

            s.Dispose();
            values.Dispose();
            hold.Dispose();
            evt.Dispose();
            behavior.Dispose();
            p.Dispose();

            foreach (var src in results)
            {
                Console.WriteLine("{0}", src);
            }
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
