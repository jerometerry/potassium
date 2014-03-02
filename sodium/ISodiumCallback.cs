namespace Sodium
{
    internal interface ISodiumCallback<in T>
    {
        void Invoke(Transaction transaction, T data);
    }
}