namespace JT.Rx.Net.Monads
{
    /// <summary>
    /// IdentityPredicate is a Predicate with a constant boolean value
    /// </summary>
    public class IdentityPredicate : Predicate
    {
        private bool predicate;

        public static IdentityPredicate True
        {
            get
            {
                return new IdentityPredicate(true);
            }
        }

        public static IdentityPredicate False
        {
            get
            {
                return new IdentityPredicate(false);
            }
        }

        public IdentityPredicate(bool predicate)
        {
            this.predicate = predicate;
        }

        public override bool Value
        {
            get
            {
                return predicate;
            }
        }
    }
}
