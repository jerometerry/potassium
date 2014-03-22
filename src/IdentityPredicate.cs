namespace JT.Rx.Net
{
    public class IdentityPredicate : Predicate
    {
        private bool predicate;

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

        public void SetValue(bool predicate)
        {
            this.predicate = predicate;
        }
    }
}
