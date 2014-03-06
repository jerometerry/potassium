Sodium.net
==========

Sodium.net is a port of (the Java implementation of) [Sodium](https://github.com/kentuckyfriedtakahe/sodium) to C#.

[Sodium](https://github.com/kentuckyfriedtakahe/sodium) is a Reactive Programming Library that has been implemented in several languages including Haskell, Java, C++, Embedded-C, and Rust. 

However, there was no implementation for .NET. Hence this project.

Check out the [wiki](https://github.com/jerometerry/sodium.net/wiki) for more information.

Sodium.net API [documentation](http://jterry.azurewebsites.net/sodium.net/)

Key Concepts
==========

**Event** - a series of discrete event occurrences.

Here's an example of an Event of int's, that writes the value to the console when the Event is fired.
```
    var e = new EventSink<int>();
    var l = e.Listen(v => Console.Write("{0} ", v));
```

**Behavior** - a continuous, time varying value. In Sodium, a Behavior is basically the latest firing of an Event.

Here's an example of a Behavior of int's with an initial value of zero, that writes the initial value to the console, and also writes the value of the behavior to the console when the underlying Event is fired.
```
    var b = new BehaviorSink<int>(0);
    var l = b.Values().Listen(v => Console.Write("{0} ", v));
```

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
