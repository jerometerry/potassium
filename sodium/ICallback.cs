namespace Sodium
{
    internal interface ICallback<in TA>
    {
        void Invoke(Transaction transaction, TA data);
    }
}