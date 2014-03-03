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
    var l = e.Listen(v => Console.WriteLine("{0}", v));
    for (var i = 0; i < 5; i++) 
        e.Fire(i);
    l.Dispose();
    e.Dispose();
```

**Map**
```
    var e = new Event<int>();
    var m = e.Map(v => v.ToString());
    var l = m.Listen(v => Console.WriteLine("{0}", v));
    for (var i = 0; i < 5; i++) 
        e.Fire(i);
    l.Dispose();
    m.Dispose();
    e.Dispose();
```

[Sodium](https://github.com/kentuckyfriedtakahe/sodium) [Copyright](https://github.com/kentuckyfriedtakahe/sodium/blob/master/COPYING)
