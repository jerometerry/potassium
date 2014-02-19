namespace sodium
{
    using System;

    public interface ITransactionHandler<in T> : IDisposable
    {
        void Run(Transaction transaction, T p);
    }
}