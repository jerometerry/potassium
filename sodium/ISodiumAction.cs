namespace Sodium
{
    internal interface ISodiumAction<in T>
    {
        void Invoke(Transaction transaction, T data);
    }
}