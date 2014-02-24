namespace Sodium
{
    // TODO - This could be replaced with Action<Transaction, TA>
    internal interface IHandler<in TA>
    {
        void Run(Transaction t, TA a);
    }
}