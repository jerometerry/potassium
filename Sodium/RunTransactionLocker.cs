namespace Sodium
{
    using System;

    internal class RunTransactionLocker : TransactionLocker
    {
        private Action<Transaction> code;

        public RunTransactionLocker(Action<Transaction> code)
        {
            this.code = code;
        }

        protected override void Apply(Transaction t)
        {
            code(t);
        }
    }
}
