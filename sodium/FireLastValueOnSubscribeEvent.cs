namespace Sodium
{
    internal class FireLastValueOnSubscribeEvent<T> : LastFiring<T>
    {
        public FireLastValueOnSubscribeEvent(IValue<T> valueStream, Transaction transaction)
            : base(new FireValueOnSubscribeEvent<T>(valueStream, transaction), transaction)
        {
            this.RegisterFinalizer(this.Source);
        }
    }
}