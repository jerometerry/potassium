namespace Sodium
{
    internal interface ITransactionHandler<in TA>
    {
        void Run(Transaction t, TA a);
    }
}