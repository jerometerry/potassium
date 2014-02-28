namespace Sodium.MemoryTests
{
    internal static class Program
    {
        public static void Main()
        {
            var test = new MemoryTest1();
            test.Test(1000000000);
        }
    }
}
