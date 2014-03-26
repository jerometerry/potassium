![alt text](https://github.com/jerometerry/potassium/raw/master/img/potassium64.png "Potassium") Potassium
====================================================================================================

Potassium is a [Functional Rective Programming (FRP)](http://www.haskell.org/haskellwiki/Functional_Reactive_Programming) Library for .Net, based on  [Sodium](https://github.com/kentuckyfriedtakahe/sodium).

FRP was introduced by [Conal Elliott](http://conal.net/) and [Paul Hudak](http://haskell.cs.yale.edu/people/paul-hudak/) in the ICFP 97 paper [Functional Reactive Animation (Fran)](http://conal.net/papers/icfp97/). 

[Conal Elliott's](http://conal.net/) answer to [What is (Functional) Reactive Programming?'](http://stackoverflow.com/questions/1028250/what-is-functional-reactive-programming) is a great resource for learning about FRP.

Potassium API documentation is located [here](http://jterry.azurewebsites.net/potassium/).

Installing
==========

Potassium is available for download on [NuGet](http://www.nuget.org/packages/Potassium/)

To install Potassium, run the following command in the [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console)

**Install-Package Potassium**

Overview
========

At the heart of FRP are **Events** and **Behaviors**.

**Events** should be familiar to programmers who've done any sort of Event Driven Programming. Mouse clicks, key presses, etc., are examples of Events. Event is technically an event stream of discrete occurrences.

The type signagure for an Event of integers is **Event&lt;int&gt;**. 

**Behaviors** are simply time varying values. Put another way, a **Behavior** is a *reactive value*. Behavior is simply a container for a value, where the value gets updated by firings of an Event.

The type signature for a Behavior of integers is **Behavior&lt;int&gt;**.

Here's an example illustrating the use of Events and Behaviors. The values **1** through **10** are fired through **evt**, while values **0** through **10** are observed. 

```
	FirableEvent<int> evt = new FirableEvent<int>();
	Behavior<int> beh = evt.Hold(0);
	Event<int> vals = beh.Values();
	ISubscription<int> s = vals.Subscribe((v) => {
		Console.WriteLine("{0}", v);
	});
	for (int i = 1; i <= 10; i++) {
		evt.Fire(i);
	}
```

**0** is written to the console because the **Behavior.Values()** generates an **Event** that, on subscription, fires the value of the **Behavior**.
