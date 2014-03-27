namespace Potassium.Providers
{
    /// <summary>
    /// IdentityPredicate is a Predicate with a constant boolean value
    /// </summary>
    public class IdentityPredicate : Predicate
    {
        private readonly bool predicate;

        /// <summary>
        /// Constructs a new, constant, predicate with the given value
        /// </summary>
        /// <param name="predicate">The value of the Predicate</param>
        public IdentityPredicate(bool predicate)
        {
            this.predicate = predicate;
        }

        /// <summary>
        /// Get the Predicate that always evaluates to True
        /// </summary>
        public static IdentityPredicate True
        {
            get
            {
                return new IdentityPredicate(true);
            }
        }

        /// <summary>
        /// Get the Predicate that always evaluates to False
        /// </summary>
        public static IdentityPredicate False
        {
            get
            {
                return new IdentityPredicate(false);
            }
        }

        /// <summary>
        /// Evaluates the value of the Provider
        /// </summary>
        public override bool Value
        {
            get
            {
                return predicate;
            }
        }
    }
}
