This is a fork of [Daniel Bradley](https://github.com/danielrbradley)'s [C# implementation](https://github.com/danielrbradley/CycleDetection) of the [Tarjan cycle detection algorithm](http://en.wikipedia.org/wiki/Tarjan's_strongly_connected_components_algorithm).

IOW: You can use this library to sort dependencies and even handle cyclic references. e.g. to compile stuff in the right order.

I found it to be quite useful but I didn't like how one had to manually setup the dependency vertices. (It also supports custom comparers now.)

So I moved the original code into the Core sub namespace and wrote a class that allows to setup dependencies using a simple lambda expression.

The easiest way to use it is to install the [nuget package](https://www.nuget.org/packages/CycleDetection).

This example shows a case in which A depends on B, B depends on C but C depends on A. Thus creating a cyclic dependency.
There's also D which depends on B. 

    // A→B
    // ↑ ↓
    // └─C

    var graph = new[]
    {
      new{Value ="D", DependsOn = "B"},
      new{Value ="A", DependsOn = "B"},
      new{Value ="B", DependsOn = "C"},
      new{Value ="C", DependsOn = "A"},
    };

    var byValue = graph.ToLookup(k => k.Value);
    var components = graph.DetectCycles(s => byValue[s.DependsOn]);

    Assert.AreEqual(2, components.Count); // 1 cycle + D
    Assert.AreEqual(1, components.IndependentComponents().Count()); // only D is outside the cycle
    Assert.AreEqual(1, components.Cycles().Count()); // 1 cycle
    Assert.AreEqual(3, components[0].Count); // the cycle has 3 components
    Assert.AreEqual("D", components[1].Single().Value); // D is after the cycle, because it depends on it

I also added a way to merge cyclic dependencies into a single entity. Which is probably not that interesting for most. 
However, I use it to have a merged assembly from every cycle of interconnected Jars when I compile them using IKVMC. And have that merged assembly name as the reference for the other Jar-assemblies.

    var mergedGraph = components.MergeCyclicDependencies((cycle, getMerged) => new
                      {
                        Value = cycle.Contents.OrderBy(t => t.Value)
                                              .Aggregate("", (r, c) => r + "-" + c.Value).TrimStart('-'),
                        DependsOn = (from d in cycle.Dependencies
                                     select (getMerged(d) ?? d.Single()).Value).SingleOrDefault()
                      });
Result

>		{ Value = "A-B-C", DependsOn = null } -> the name reflects  all included items
>		{ Value = "D", DependsOn = "A-B-C" }  -> D now references the new name (thx to "getMerged")


Original Readme:

> Just decided to put together a c# project for an implementation of the Tarjan cycle detection algorithm as I can't seem to find any out there already.
> 
> The original code for this was posted on [stackoverflow](http://stackoverflow.com/questions/6643076/tarjan-cycle-detection-help-c-sharp) by [user623879](http://stackoverflow.com/users/623879/user623879). 
> 
> Hope this is useful!
