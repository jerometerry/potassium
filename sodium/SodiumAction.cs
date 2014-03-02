namespace Sodium
{
    using System;

    internal sealed class SodiumAction<T> : ISodiumAction<T>
    {
        private readonly Action<Transaction, T> action;

        public SodiumAction(Action<Transaction, T> action)
        {
            this.action = action;
        }

        public void Invoke(Transaction transaction, T data)
        {
            action(transaction, data);
        }
    }
}