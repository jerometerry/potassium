namespace Sodium
{
    internal interface ISodiumAction<in TA>
    {
        void Invoke(Transaction transaction, TA data);
    }
}