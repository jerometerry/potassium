namespace Sodium
{
    using System;

    internal sealed class Runnable : IRunnable
    {
        private readonly Action action;

        public Runnable(Action action)
        {
            this.action = action;
        }

        public void Run()
        {
            action();
        }
    }
}