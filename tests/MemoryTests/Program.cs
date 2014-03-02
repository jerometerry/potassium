namespace Sodium.MemoryTests
{
    internal static class Program
    {
        public static void Main()
        {
            //var test = new MemoryTest1();
            //test.Test(1000000000);

            Event<int> evt = new Event<int>();
            var l = evt.Listen(t => { });

            for (int i = 0; i < 1000000000; i++)
            {
                evt.Fire(i);
            }

            evt.Dispose();
            l.Dispose();
        }
    }
}
