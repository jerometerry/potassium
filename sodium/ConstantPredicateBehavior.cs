namespace Sodium
{
    public class ConstantPredicateBehavior : PredicateBehavior
    {
        private bool predicate;

        public ConstantPredicateBehavior(bool predicate)
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
