This is a fork of [Daniel Bradley](https://github.com/danielrbradley)'s [C# implementation](https://github.com/danielrbradley/CycleDetection) of the [Tarjan cycle detection algorithm](http://en.wikipedia.org/wiki/Tarjan's_strongly_connected_components_algorithm).

I found it to be quite useful but I didn't like how one had to manually setup the dependency vertices. (It also supports custom comparers now.)

So I moved the original code into the Core sub namespace and wrote a class that allows to setup dependencies using a simple lambda expression.

This is example is taken from the test Cycle2. It shows a case in which A depends on B, B depends on C but C depends on A. Thus creating a cyclic dependency.

    // A→B
    // ↑ ↓
    // └─C

    var graph = new[]
    {
      new{Value ="A", DependsOn = "B"},
      new{Value ="B", DependsOn = "C"},
      new{Value ="C", DependsOn = "A"},
    };

    var byValue = graph.ToLookup(k => k.Value);
    var components = graph.DetectCycles(s => byValue[s.DependsOn]);

    Assert.AreEqual(1, components.Count); // 1 cycle
    Assert.AreEqual(0, components.IndependentComponents().Count()); // no component outside that 1 cycle
    Assert.AreEqual(1, components.Cycles().Count()); // 1 cycle
    Assert.AreEqual(3, components.Single().Count); // the cycle has 3 components

Original Readme:

> Just decided to put together a c# project for an implementation of the Tarjan cycle detection algorithm as I can't seem to find any out there already.
> 
> The original code for this was posted on [stackoverflow](http://stackoverflow.com/questions/6643076/tarjan-cycle-detection-help-c-sharp) by [user623879](http://stackoverflow.com/users/623879/user623879). 
> 
> Hope this is useful!
