Sodium.net
==========

Sodium.net is a port of (the Java implementation of) [Sodium](https://github.com/kentuckyfriedtakahe/sodium) to C#.

[Sodium](https://github.com/kentuckyfriedtakahe/sodium) is a Reactive Programming Library that has been implemented in several languages including Haskell, Java, C++, Embedded-C, and Rust. 

However, there was no implementation for .NET. Hence this project.

Check out the [wiki](https://github.com/jerometerry/sodium.net/wiki) for more information.

Sodium.net API [documentation](http://jterry.azurewebsites.net/sodium.net/)

Event Examples
==========

**Echo**
```
    var e = new EventSink<int>();
    var l = e.Listen(v => Console.Write("{0} ", v));
    for (var i = 0; i < 5; i++) 
        e.Fire(i);
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
        e.Fire(i);
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
            e1.Fire(i);
        }
        else {
            e2.Fire(i);
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
    e.Fire('H');
    e.Fire('o');
    e.Fire('I');
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
        b.Fire(i);
    l.Dispose();
    b.Dispose();
```
*Output*
```
    0 1 2 3 4
```

**Event to Behavior**
```
    var e = new EventSink<int>();
    var b = e.ToBehavior(0);
    var l = b.Listen(v => Console.Write("{0} ", v));
    for (var i = 1; i <= 5; i++) 
        e.Fire(i);
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
    var b = e.ToBehavior(0);
    var v = b.Values();
    var l = v.Listen(v => Console.Write("{0} ", v));
    for (var i = 1; i <= 5; i++) 
        e.Fire(i);
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
