namespace Sodium
{
    using System;

    /// <summary>
    /// An Event is a series of discrete occurrences
    /// </summary>
    /// <typeparam name="T">The type of value that will be published through the Event</typeparam>
    public class Event<T> : Observable<T>
    {
    }
}
