Sodium.net
==========

Sodium.net is a port of (the Java implementation of) [Sodium](https://github.com/kentuckyfriedtakahe/sodium) to C#.

[Sodium](https://github.com/kentuckyfriedtakahe/sodium) is a Reactive Programming Library that has been implemented in several languages including Haskell, Java, C++, Embedded-C, and Rust. 

However, there was no implementation for .NET. Hence this project.

Check out the [wiki](https://github.com/jerometerry/sodium.net/wiki) for more information.

Sodium.net API [documentation](http://jterry.azurewebsites.net/sodium.net/)

Overview
==========

Sodium is made up of 2 primitives with 12 opeartions.

The primitives are Event, and Behavior.

The operations are Accum, Apply, Coalesce, Collect, Filter, Gate, Hold, Lift, Map, Merge, Snapshot, Switch


Key Classes
==========

**Event** - a series of discrete event occurrences.

Here's a code snippit for listening to an Event of int's.
```
    Event<int> e; // obtained from a series of operations on Events / Behaviors
    ...
    var l = e.Listen(v => Console.Write("{0} ", v));
```

**Behavior** - a continuous, time varying value. In Sodium, a Behavior is composed of a value, and an Event that is listened to to update the value. The Event inside the Behavior is accessed via the *Source* Property.

Here's a code snippit for listeneing to a Behavior of int's. 
```
    Behavior<int> b; // obtained from a series of operations on Events / Behaviors
    ...
    var l = b.Listen(v => Console.Write("{0} ", v));
``` 

**EventSink** - EventSink is an Event that can be fired (the name of the firing method in Sodium is *Send*).

Here's an example of creating an EventSink, and sending (aka firing) a value to all registered listeners.
```
    var e = new EventSink<int>();
    ... // register listeners
    e.Send(0);
```

**BehaviorSink** - BehaviorSink is a Behavior that can be fired

Here's an example of constructing a BehaviorSink with an initial value of zero, and seding it a new value of one,  after some operations that register listeners.
```
    var b = new BehaviorSink<int>(0);
    ... // register listeners
    b.Send(1);
```

**EventLoop** - EventLoop is an EventSink that listenes another Event for firings, and fires the same value on itself, known as looping. 

Below is a basic example of using the EventLoop. Firing happens on *e*, which triggers the callback on *loop*, writing the value to the console.
```
    var e = new EventSink<int>(); 
    var loop = new EventLoop<int>();
    loop.Loop(e);
    var l = loop.Listen(v => Console.Write("{0} ", v);
    e.Fire(0);
```

**BehaviorLoop** - BehaviorLoop loops firings from one Behavior to another, similar to EventLoop (internally, Behavior just calls the EventLoop.Loop method).

Below is a basic example of using the BehaviorLoop. Firing happens on *b*, which triggers the callback on *loop*, writing the value to the console.
```
    var b = new BehaviorSink<int>(0);
    var loop = new BehaviorLoop<int>();
    loop.Loop(b);
    var l = loop.Listen(v => Console.Write("{0} ", v);
    e.Fire(1);
```

Key Operations
==========

**Hold** - A Hold is an operation on an Event given an initial value, that creates a Behavior with the initial value that updates whenever the Event fires.

**Map** - A map is the process of converting an Event or Behavior from one type to another, by supplying a mapping function.

**Filter** - Filtering is the process of throwing away any firings from source Events that don't evaluate to true through a given filter predicate.

**Gate** - Gating is the process of now allowing Firing values that don't evaluate to true through a given gate predicate.

**Merge** - Merging is the process of combining two Events of the same type into a single Event, using a coalesce (combining) function.

**Snapshot** - Snapshot is the process of sampling a Behaviors value at the time of an Event firing, and producing a value (a snapshot) by passing the value of the Behavior at the time of the firing and the value fired on the Event into a snapshot function.

**Apply** - Apply is the process of taking a Behavior and a Behavior of mapping functions, to produce of a new Behavior of the return type of the mapping function inside the Behavior. This is the primitive for Behavior lifting.

**Lift** - Lifting is the process of taking a multi-valued function (2 or 3 valued functions in Sodium), along with Behaviors for each value of the function, and creating a new Behavior that is computed by evaluating the function with the current values of each of the Behaviors values.

A lifted Behavior has the property that if any of the input Behaviors are modified simultaneously, the lifted Behavior should fire only once.

**Lift Glitch** - A lift glitch occurs if a lifted Behavior fires multiple times in response to simultaneous updates of it's input Behaviors.

**Coalesce** - Coalesce is the process of combining several simultaneous values into a single value. 

**Switch** - Switch is the process of unwrapping a Behavior of Behaviors or a Behavior of Events into the inner Behavior or Event. Switch allows the reactive network to change dynamically, using reactive logic to modify reactive logic.

**Transaction** - A Transaction is used to provide the concept of simultaneous Events. 

In Sodium, only one Transaction can be open at a time. A Transaction is requested when registering a listener, or when firing a value. A Transaction is closed when the initial request that created the Transaction completes. 

Firing and listening can cause a chain reaction of operations that may cause other firing and listening operations. This chain of operations will all execute inside the same Transaction. 

Once a Transaction has been opened, actions can be added to it, using the High, Medium and Low methods. 

High registers the action on a Priority Queue using a Rank to order. Medium and Low are run in the order they are added.

All High priority actions are run first (using Priority Queue), all Medium Priority actions second (by order added), and all Low priority actions third (by order added). 

High, Medium and Low priority actions are run when the Transaction is closed when the operation that requested the Transaction completes.

**Rank** - Rank is used to determine the order to execute High priority items in a Transaction. A lower priority value executes before a higher priority, as is typically with Priority Queues.

A Rank is assigned to an Event, with a default priority of zero. A Rank has an operation to add a superior, with a class invariant that all superiors must have a higher priority than their subordinates. 

An Events Rank can be modified (increased) when it registers itself to listen to another Event. The listeners rank should be higher than the source rank, so that the source rank High operations in a Transaction execute before those of their listeners.

The Rank of an Event is used when registering High priority actions on a Transaction, as a means to ensure actions are run on a priority basis.

Memory Management
==========

```Event```, ```Behavior``` and ```IEventListener``` all implement ```IDisposable```, and all should be disposed of when you are done with them. 

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
==========

**Echo**
```
    var e = new EventSink<int>();
    var l = e.Listen(v => Console.Write("{0} ", v));
    for (var i = 0; i < 5; i++) 
        e.Send(i);
    l.Dispose();
    e.Dispose();
```
*Output*
```
    0 1 2 3 4
```


**Map**
```
    var e = new EventSink<int>();
    var m = e.Map(v => v.ToString());
    var l = m.Listen(v => Console.Write("{0} ", v));
    for (var i = 0; i < 5; i++) 
        e.Send(i);
    l.Dispose();
    m.Dispose();
    e.Dispose();
```
*Output*
```
    0 1 2 3 4
```

**Merge**
```
    var e1 = new EventSink<int>();
    var e2 = new EventSink<int>();
    var m = e1.Merge(e2);
    var l = m.Listen(v => Console.Write("{0} ", v));
    for (var i = 0; i < 10; i++) {
        if (i % 2 == 0) {
            e1.Send(i);
        }
        else {
            e2.Send(i);
        }
    }
    l.Dispose();
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
    var e = new EventSink<char>();
    var f = e.Filter(c => char.IsUpper(c));
    var l = f.Listen(v => Console.Write("{0} ", v));
    e.Send('H');
    e.Send('o');
    e.Send('I');
    l.Dispose();
```
*Output*
```
    H I
```

Behavior Examples
==========

**Echo**
```
    var b = new BehaviorSink<int>(0);
    var l = b.Listen(v => Console.Write("{0} ", v));
    for (var i = 0; i < 5; i++) 
        b.Send(i);
    l.Dispose();
    b.Dispose();
```
*Output*
```
    0 1 2 3 4
```

**Hold**
```
    var e = new EventSink<int>();
    var b = e.Hold(0);
    var l = b.Listen(v => Console.Write("{0} ", v));
    for (var i = 1; i <= 5; i++) 
        e.Send(i);
    l.Dispose();
    b.Dispose();
    e.Dispose();
```
*Output*
```
    1 2 3 4 5
```

**Values**
```
    var e = new EventSink<int>();
    var b = e.Hold(0);
    var v = b.Values();
    var l = v.Listen(v => Console.Write("{0} ", v));
    for (var i = 1; i <= 5; i++) 
        e.Send(i);
    l.Dispose();
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
