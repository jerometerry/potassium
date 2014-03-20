namespace Sodium
{
    /// <summary>
    /// BehaviorFeed is a Behavior that can be fed values from another Behavior
    /// </summary>
    /// <typeparam name="T">The type of values published through the Behavior</typeparam>
    /// <remarks>A BehaviorFeed can only be fed values from single Behavior. 
    /// If Feed is Invoked multiple times, an exception will be raised.</remarks>
    public class BehaviorFeed<T> : Behavior<T>
    {
        /// <summary>
        /// Constructs a new BehaviorFeed
        /// </summary>
        /// <param name="value">The initial value of the Behavior</param>
        public BehaviorFeed(T value)
            : base(new EventFeed<T>(), value)
        {
            this.Register(this.Source);
        }

        /// <summary>
        ///  Firings on the given Event will be fed to the current Event
        /// </summary>
        /// <param name="source">Event who's publishings will be looped to the current Event</param>
        public void Feed(Behavior<T> source)
        {
            var eventFeed = (EventFeed<T>)this.Source;
            eventFeed.Feed(source.Source);
        }
    }
}
