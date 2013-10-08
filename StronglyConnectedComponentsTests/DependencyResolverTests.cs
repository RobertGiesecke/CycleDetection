using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StronglyConnectedComponents.Core;

namespace StronglyConnectedComponents.Tests
{
  [TestClass]
  public class DependencyResolverTests
  {
    [TestMethod]
    public void EmptyGraph()
    {
      var graph = new List<Vertex<int>>();
      var detector = new StronglyConnectedComponentFinder<int>();
      var cycles = detector.DetectCycle(graph);
      Assert.AreEqual(0, cycles.Count);
    }

    // A
    [TestMethod]
    public void SingleVertex()
    {
      var graph = new List<int>();
      graph.Add(1);
      var components = graph.DetectCycles(t => null);
      Assert.AreEqual(1, components.Count);
      Assert.AreEqual(1, components.IndependentComponents().Count());
      Assert.AreEqual(0, components.Cycles().Count());
    }

    // A→B

    static class TestValue
    {
      public static TestValue<T> Create<T>(T value, params T[] dependsOn)
      {
        return new TestValue<T>(value, dependsOn);
      }
    }

    public class TestValue<T>
    {
      public T Value { get; private set; }
      public IList<T> DependsOn { get; private set; }

      public TestValue(T value, params T[] dependsOn)
      {
        Value = value;
        DependsOn = new ReadOnlyCollection<T>(dependsOn);
      }
    }

    [TestMethod]
    public void Linear2()
    {
      var graph = new List<TestValue<int>>
        {
          TestValue.Create(1, 2), 
          TestValue.Create(2)
        };

      var byValue = graph.ToLookup(t => t.Value);

      var components = graph.DetectCycles(s => s.DependsOn.SelectMany(d => byValue[d]));
      Assert.AreEqual(2, components.Count);
      Assert.AreEqual(2, components.IndependentComponents().Count());
      Assert.AreEqual(0, components.Cycles().Count());
    }

    // A→B→C
    [TestMethod]
    public void Linear3()
    {
      var graph = new[]
        {
          TestValue.Create(1, 2),
          TestValue.Create(2, 3),
          TestValue.Create(3)
        };
      var byValue = graph.ToLookup(t => t.Value);

      var components = graph.DetectCycles(s => s.DependsOn.SelectMany(d => byValue[d]));

      Assert.AreEqual(3, components.Count);
      Assert.AreEqual(3, components.IndependentComponents().Count());
      Assert.AreEqual(0, components.Cycles().Count());
    }

    // A↔B
    [TestMethod]
    public void Cycle2()
    {
      var graph = new[]
        {
          TestValue.Create(1, 2),
          TestValue.Create(2, 1),
        };
      var byValue = graph.ToLookup(t => t.Value);
      var components = graph.DetectCycles(s => s.DependsOn.SelectMany(d => byValue[d]));

      Assert.AreEqual(1, components.Count);
      Assert.AreEqual(0, components.IndependentComponents().Count());
      Assert.AreEqual(1, components.Cycles().Count());
      Assert.AreEqual(2, components.First().Count());
    }

    [TestMethod]
    public void Cycle3()
    {
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

      Assert.AreEqual(1, components.Count);
      Assert.AreEqual(0, components.IndependentComponents().Count());
      Assert.AreEqual(1, components.Cycles().Count());
      Assert.AreEqual(3, components.Single().Count);
    }

    // A→B   D→E
    // ↑ ↓   ↑ ↓
    // └─C   └─F
    [TestMethod]
    public void TwoIsolated3Cycles()
    {
      var graph = new[]
        {
          TestValue.Create(1, 2),
          TestValue.Create(2, 3),
          TestValue.Create(3, 1),
          
          TestValue.Create(4, 5),
          TestValue.Create(5, 6),
          TestValue.Create(6, 4),
        };
      var byValue = graph.ToLookup(t => t.Value);
      var components = graph.DetectCycles(s => s.DependsOn.SelectMany(d => byValue[d]));

      Assert.AreEqual(2, components.Count);
      Assert.AreEqual(0, components.IndependentComponents().Count());
      Assert.AreEqual(2, components.Cycles().Count());
      Assert.IsTrue(components.All(c => c.Count == 3));
    }

    // A→B
    // ↑ ↓
    // └─C-→D
    [TestMethod]
    public void Cycle3WithStub()
    {
      TestValue<int> vD;
      var graph = new[]
        {
          TestValue.Create(1, 2),
          TestValue.Create(2, 3),
          TestValue.Create(3, 1, 4),
          vD=TestValue.Create(4),
        };
      var byValue = graph.ToLookup(t => t.Value);
      var components = graph.DetectCycles(s => s.DependsOn.SelectMany(d => byValue[d]));

      Assert.AreEqual(2, components.Count);
      Assert.AreEqual(1, components.IndependentComponents().Count());
      Assert.AreEqual(1, components.Cycles().Count());
      Assert.AreEqual(1, components.Count(c => c.Count == 3));
      Assert.AreEqual(1, components.Count(c => c.Count == 1));
      Assert.IsTrue(components.Single(c => c.Count == 1).Single() == vD);
    }
  }
}
