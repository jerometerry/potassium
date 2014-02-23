namespace Sodium
{
    internal class ApplyTransactionLocker<TA> : TransactionLocker
    {
        private ILambda1<Transaction, TA> code;
        private TA result;

        public ApplyTransactionLocker(ILambda1<Transaction, TA> code)
        {
            this.code = code;
        }

        public TA Result
        {
            get { return result; }
        }

        protected override void Apply(Transaction t)
        {
            this.result = code.Apply(t);
        }
    }
}
