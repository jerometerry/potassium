namespace Sodium
{
    using System;

    internal sealed class Handler<TA> : IHandler<TA>
    {
        private readonly Action<Transaction, TA> action;

        public Handler(Action<Transaction, TA> action)
        {
            this.action = action;
        }

        public void Run(Transaction trans, TA a)
        {
            action(trans, a);
        }
    }
}