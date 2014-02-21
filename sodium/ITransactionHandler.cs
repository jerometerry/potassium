namespace sodium
{
    public interface ITransactionHandler<A>
    {
        void Run(Transaction trans, A a);
    }
}

