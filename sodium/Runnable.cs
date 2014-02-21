using System;

namespace sodium
{
    public class Runnable : IRunnable
    {
        private readonly Action _action;

        public Runnable(Action action)
        {
            this._action = action;
        }

        public void Run()
        {
            _action();
        }
    }
}