namespace Sodium
{
    internal class RunTransactionLocker : TransactionLocker
    {
        private IHandler<Transaction> code;

        public RunTransactionLocker(IHandler<Transaction> code)
        {
            this.code = code;
        }

        protected override void Apply(Transaction t)
        {
            code.Run(t);
        }
    }
}
