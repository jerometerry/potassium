![alt text](https://github.com/jerometerry/potassium/raw/master/img/potassium64.png "Potassium") Potassium
====================================================================================================

Potassium is a Functional Rective Programming Library for .NET, based on [Sodium](https://github.com/kentuckyfriedtakahe/sodium).

Potassium API [documentation](http://jterry.azurewebsites.net/potassium/)

Installing
==========

Potassium is available for download on [NuGet](http://www.nuget.org/packages/Potassium/)

To install Potassium, run the following command in the [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console)

**Install-Package Potassium**

Background
==========

Functional Reactive Programming (FRP) was introduced by [Conal Elliott](http://conal.net/) and [Paul Hudak](http://haskell.cs.yale.edu/people/paul-hudak/) in the ICFP 97 paper [Functional Reactive Animation (Fran)](http://conal.net/papers/icfp97/).

What is (functional) reactive programming?
==========================================

[Conal Elliott's](http://conal.net/) answer on [Stack Overflow](http://stackoverflow.com/questions/1028250/what-is-functional-reactive-programming) says it best.

Potassium API
=============

Potassium is made up of 4 primitives with 14 opeartions.

The primitives are: 

**Behavior** - A dynamic value that updates via an Event

**Event** - A stream of values (event occurrences) that can be observed

**FirableEvent** - An Event that exposes a Fire method

**EventFeed** - EventFeed fires values observed from another Event

The operations are Accum, Apply, Coalesce, Collect, Feed, Filter, Fire, Gate, Hold, Lift, Map, Merge, Snapshot, Switch

Potassium uses the Publish / Subscribe Pattern.

Key Classes
===========

**Event**

Here's a code snippit for listening to an Event of int's.
```
    Event<int> e; // obtained from a series of operations on Events / Behaviors
    ...
    var s = e.Subscribe(v => Console.Write("{0} ", v));
```

**Behavior**

Here's a code snippit for listeneing to a Behavior of int's. 
```
    Behavior<int> b; // obtained from a series of operations on Events / Behaviors
    ...
    var s = b.Subscribe(v => Console.Write("{0} ", v));
``` 

**FirableEvent** - FirableEvent is an Event that exposes a Publish method, allowing callers to fire values to it's subscribers.

Here's an example of creating an FirableEvent, and publishing a value to all registered subscribers.
```
    var e = new FirableEvent<int>();
    ... // register subscribers
    e.Publish(0);
```

**EventFeed** - EventFeed is an FirableEvent that fed values from another Event. 

Below is a basic example of using the EventFeed. Publishing happens on *e*, which triggers the callback on *feed*, writing the value to the console.
```
    var e = new FirableEvent<int>(); 
    var feed = new EventFeed<int>();
    feed.Feed(e);
    var s = feed.Subscribe(v => Console.Write("{0} ", v);
    e.Publish(0);
```

Key Operations
==========

**Accum** - Accum accumulates values into a Behavior, starting with an initial value, and applying an accumulator function on the previous value and the newly fired value to create the new value. The result of Accum is a Behavior that accumulates a value.
                        
An example would having a running sum total, starting with an initial value and using the addition operation as the snapshot.

**Apply** - Apply is the process of taking a Behavior and a Behavior of mapping functions, to produce of a new Behavior of the return type of the mapping function inside the Behavior. This is the primitive for Behavior lifting.

**Coalesce** - Coalesce is the process of combining several simultaneous values into a single value. 

**Collect** - Collect is similar to Accum, except the output is an Event, and state is maintained by storing the mapped value and the snapshot in a Tuple. The result of a Collect is a new Event that fires the accumulated value when the source Event fires.

**Feed** - Feeding is a way of chaining together Events. Given an Event e and an EventFeed f, f.Feed(e) will cause firings on e to be fired on f.

**Filter** - Filtering is the process of throwing away any publishings from source Events that don't evaluate to true through a given filter predicate.

**Fire** - Firing is the process of sending a value through an Event.

**Gate** - Gating is the process of now allowing Firing values that don't evaluate to true through a given gate predicate.

**Hold** - A Hold is an operation on an Event given an initial value, that creates a Behavior with the initial value that updates whenever the Event publishes.

**Lift** - Lifting is the process of taking a multi-valued function (2 or 3 valued functions in Potassium), along with Behaviors for each value of the function, and creating a new Behavior that is computed by evaluating the function with the current values of each of the Behaviors values.

A lifted Behavior has the property that if any of the input Behaviors are modified simultaneously, the lifted Behavior should fire only once.

**Lift Glitch** - A lift glitch occurs if a lifted Behavior publishes multiple times in response to simultaneous updates of it's input Behaviors.

**Map** - A map is the process of converting an Event or Behavior from one type to another, by supplying a mapping function.

**Merge** - Merging is the process of combining two Events of the same type into a single Event, using a coalesce (combining) function.

**Snapshot** - Snapshot is the process of sampling a Behaviors value at the time of an Event publishing, and producing a value (a snapshot) by passing the value of the Behavior at the time of the publishing and the value published on the Event into a snapshot function.

**Switch** - Switch is the process of unwrapping a Behavior of Behaviors or a Behavior of Events into the inner Behavior or Event. Switch allows the reactive network to change dynamically, using reactive logic to modify reactive logic.

**Transaction** - A Transaction is used to provide the concept of simultaneous Events. 

In Potassium, only one Transaction can be open at a time. A Transaction is requested when registering a listener, or when publishing a value. A Transaction is closed when the initial request that created the Transaction completes. 

Firing and listening can cause a chain reaction of operations that may cause other publishing and listening operations. This chain of operations will all execute inside the same Transaction. 

Once a Transaction has been opened, actions can be added to it, using the High, Medium and Low methods. 

High registers the action on a Priority Queue using a Priority to order. Medium and Low are run in the order they are added.

All High priority actions are run first (using Priority Queue), all Medium Priority actions second (by order added), and all Low priority actions third (by order added). 

High, Medium and Low priority actions are run when the Transaction is closed when the operation that requested the Transaction completes.

**Priority** - Priority is used to determine the order to execute High priority items in a Transaction. A lower priority value executes before a higher priority, as is typically with Priority Queues.

A Priority is assigned to an Event, with a default priority of zero. A Priority has an operation to add a superior, with a class invariant that all superiors must have a higher priority than their subordinates. 

An Events Priority can be modified (increased) when it registers itself to listen to another Event. The subscribers priority should be higher than the source rank, so that the source rank High operations in a Transaction execute before those of their subscribers.

The Priority of an Event is used when registering High priority actions on a Transaction, as a means to ensure actions are run on a priority basis.

Memory Management
==========

```Event```, ```Behavior``` and ```ISubscription``` all implement ```IDisposable```, and all should be disposed of when you are done with them. 

Return values of ```Event``` and ```Behavior``` methods that return other ```Events``` and ```Behaviors``` also need to be disposed of. 

This is especially important if you are calling methods on ```Events``` and / or ```Behaviors``` inside callback functions.

Here's an example, extraced from *MemoryTest1.cs*

```
    public void Test() {
        var behaviorMapFinalizers = new List<IDisposable>();

        var eventOfBehaviors = changeTens.Map(tens => {
            DisposeFinalizers(behaviorMapFinalizers);
            var mapped = behavior.Map(tt => new Tuple<int?, int?>(tens, tt));
            behaviorMapFinalizers.Add(mapped);
            return mapped;
        });
    }

    private static void DisposeFinalizers(List<IDisposable> items) {
        foreach (var item in items)
            item.Dispose();
        items.Clear();
    }
```

Notice the use of ```DisposeFinalizers``` inside the ```changeTens.Map``` lambda expression. ```Behavior.Map``` creates a new instance of ```Behavior```, which needs to be disposed. Without this, there would be a memory leak.

Event Examples
==============

**Echo**
```
    var e = new FirableEvent<int>();
    var s = e.Subscribe(v => Console.Write("{0} ", v));
    for (var i = 0; i < 5; i++) 
        e.Publish(i);
    s.Dispose();
    e.Dispose();
```
*Output*
```
    0 1 2 3 4
```


**Map**
```
    var e = new FirableEvent<int>();
    var m = e.Map(v => v.ToString());
    var s = m.Subscribe(v => Console.Write("{0} ", v));
    for (var i = 0; i < 5; i++) 
        e.Publish(i);
    s.Dispose();
    m.Dispose();
    e.Dispose();
```
*Output*
```
    0 1 2 3 4
```

**Merge**
```
    var e1 = new FirableEvent<int>();
    var e2 = new FirableEvent<int>();
    var m = e1.Merge(e2);
    var s = m.Subscribe(v => Console.Write("{0} ", v));
    for (var i = 0; i < 10; i++) {
        if (i % 2 == 0) {
            e1.Publish(i);
        }
        else {
            e2.Publish(i);
        }
    }
    s.Dispose();
    m.Dispose();
    e2.Dispose();
    e1.Dispose();
```
*Output*
```
    0 1 2 3 4 6 7 8 9
```

**Filter**
```
    var e = new FirableEvent<char>();
    var f = e.Filter(c => char.IsUpper(c));
    var s = f.Subscribe(v => Console.Write("{0} ", v));
    e.Publish('H');
    e.Publish('o');
    e.Publish('I');
    s.Dispose();
```
*Output*
```
    H I
```

Behavior Examples
=================

**Hold**
```
    var e = new FirableEvent<int>();
    var b = e.Hold(0);
    var s = b.Source.Subscribe(v => Console.Write("{0} ", v));
    for (var i = 1; i <= 5; i++) 
        e.Publish(i);
    s.Dispose();
    b.Dispose();
    e.Dispose();
```
*Output*
```
    1 2 3 4 5
```

**Values**
```
    var e = new FirableEvent<int>();
    var b = e.Hold(0);
    var v = b.Values();
    var s = v.Subscribe(v => Console.Write("{0} ", v));
    for (var i = 1; i <= 5; i++) 
        e.Publish(i);
    s.Dispose();
    v.Dispose();
    b.Dispose();
    e.Dispose();
```
*Output*
```
    0 1 2 3 4 5
```

==========
[Sodium](https://github.com/kentuckyfriedtakahe/sodium) [Copyright](https://github.com/kentuckyfriedtakahe/sodium/blob/master/COPYING)
