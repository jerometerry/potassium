namespace Sodium
{
    using System;

    public sealed class Handler<TA> : IHandler<TA>
    {
        private readonly Action<TA> action;

        public Handler(Action<TA> action)
        {
            this.action = action;
        }

        public void Run(TA a)
        {
            action(a);
        }
    }
}