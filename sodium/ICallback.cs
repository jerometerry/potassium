namespace Sodium
{
    // TODO - This could be replaced with Action<Transaction, TA>
    internal interface ICallback<in TA>
    {
        void Invoke(Transaction transaction, TA data);
    }
}