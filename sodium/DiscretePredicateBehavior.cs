namespace Sodium
{
    /// <summary>
    /// DiscretePredicateBehavior is a discrete Behavior containing a boolean value, 
    /// that can be used as a predicate.
    /// </summary>
    public class DiscretePredicateBehavior : PredicateBehavior
    {
        private bool predicate;

        public DiscretePredicateBehavior(bool predicate)
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

        public void SetValue(bool predicate)
        {
            this.predicate = predicate;
        }
    }
}
