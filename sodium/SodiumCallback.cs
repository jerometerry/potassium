namespace Sodium
{
    using System;

    internal sealed class SodiumCallback<T> : ISodiumCallback<T>
    {
        private readonly Action<Transaction, T> action;

        public SodiumCallback(Action<Transaction, T> action)
        {
            this.action = action;
        }

        public void Invoke(Transaction transaction, T data)
        {
            action(transaction, data);
        }
    }
}