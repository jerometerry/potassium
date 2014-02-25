namespace Sodium
{
    using System;

    internal sealed class Callback<TA> : ICallback<TA>
    {
        private readonly Action<Transaction, TA> action;

        public Callback(Action<Transaction, TA> action)
        {
            this.action = action;
        }

        public void Invoke(Transaction transaction, TA data)
        {
            action(transaction, data);
        }
    }
}