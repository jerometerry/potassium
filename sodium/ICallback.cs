namespace Sodium
{
    public interface ICallback<in TA>
    {
        void Invoke(Transaction transaction, TA data);
    }
}