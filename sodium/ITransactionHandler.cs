namespace Sodium
{
    public interface ITransactionHandler<in TA>
    {
        void Run(Transaction trans, TA a);
    }
}