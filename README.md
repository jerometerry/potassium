Sodium.net
==========

Sodium.net is a port of (the Java implementation of) [Sodium](https://github.com/kentuckyfriedtakahe/sodium) to C#.

[Sodium](https://github.com/kentuckyfriedtakahe/sodium) is a Reactive Programming Library that has been implemented in several languages including Haskell, Java, C++, Embedded-C, and Rust. 

However, there was no implementation for .NET. Hence this project.

Check out the [wiki](https://github.com/jerometerry/sodium.net/wiki) for more information.

Sodium.net API [documentation](http://jterry.azurewebsites.net/sodium.net/)

Examples
==========

**Echo**
```
    var e = new Event<int>();
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
    var e = new Event<int>();
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
    var e1 = new Event<int>();
    var e2 = new Event<int>();
    var m = e1.Merge(e2);
    var l = m.Listen(v => Console.Write("{0} ", v));
    for (var i = 0; i < 5; i++)
        e1.Fire(i);
    for (var i = 6; i < 10; i++ )
        e2.Fire(i);
    l.Dispose();
    m.Dispose();
    e2.Dispose();
    e1.Dispose();
```
*Output*
```
    0 1 2 3 4 5 6 7 8 9
```

==========
[Sodium](https://github.com/kentuckyfriedtakahe/sodium) [Copyright](https://github.com/kentuckyfriedtakahe/sodium/blob/master/COPYING)
