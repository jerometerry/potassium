namespace Sodium
{
    // TODO - This could be replaced with Action<Transaction, TA>
    internal interface ITrigger<in TA>
    {
        void Fire(Transaction t, TA a);
    }
}