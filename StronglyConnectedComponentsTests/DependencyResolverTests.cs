using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StronglyConnectedComponents.Core;
using StronglyConnectedComponents.Tests.Utils;

namespace StronglyConnectedComponents.Tests
{
  [TestClass]
  public class DependencyResolverTests : TestBase
  {
    [TestMethod]
    public void EmptyGraph()
    {
      var graph = new List<int>();
      var cycles = graph.DetectCycles(t => null);
      Assert.AreEqual(0, cycles.Count);
    }

    // A
    [TestMethod]
    public void SingleVertex()
    {
      var graph = new[]
                  {
                    1
                  };
      var components = graph.DetectCycles(t => null);
      Assert.AreEqual(1, components.Count);
      Assert.AreEqual(1, components.IndependentComponents().Count());
      Assert.AreEqual(0, components.Cycles().Count());
    }

    // A→B

    [TestMethod]
    public void Linear2()
    {
      var graph = new[]
                  {
                    TestValue.Create(1, 2),
                    TestValue.Create(2)
                  };

      var byValue = graph.ToLookup(t => t.Value);

      var components = graph.DetectCyclesUsingKey(t => t.Value, s => s.DependsOn);
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

      var components = graph.DetectCyclesUsingKey(t => t.Value, s => s.DependsOn);

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
      var components = graph.DetectCyclesUsingKey(t => t.Value, s => s.DependsOn);

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
                    new
                    {
                      Value = "A",
                      DependsOn = "B"
                    },
                    new
                    {
                      Value = "B",
                      DependsOn = "C"
                    },
                    new
                    {
                      Value = "C",
                      DependsOn = "A"
                    },
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
      var components = graph.DetectCyclesUsingKey(t => t.Value, s => s.DependsOn);

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
      TestValue<string> vD;
      var graph = new[]
                  {
                    TestValue.Create("A", "B"),
                    TestValue.Create("B", "C"),
                    TestValue.Create("C", "A", "D"),
                    vD = TestValue.Create("D"),
                  };
      var components = graph.DetectCyclesUsingKey(t => t.Value, s => s.DependsOn);

      Assert.AreEqual(2, components.Count);
      Assert.AreEqual(1, components.IndependentComponents().Count());
      Assert.AreEqual(1, components.Cycles().Count());
      Assert.AreEqual(1, components.Count(c => c.Count == 3));
      Assert.AreEqual(1, components.Count(c => c.Count == 1));
      Assert.IsTrue(components.Single(c => c.Count == 1).Single() == vD);
    }


    // This test verifies that the correct equality comparer is used to identify values
    //       E
    //       ↓
    // A(a )→B(b)
    // ↑ ↓
    // └─C( c )-→D
    [TestMethod]
    public void Cycle4WithStubIgnoreCaseAndTrim()
    {
      var testValues = new
                       {
                         A = TestValue.Create("a ", "B"),
                         B = TestValue.Create(" B", " c "),
                         C = TestValue.Create("C", "A", "D"),
                         D = TestValue.Create("d"),
                         E = TestValue.Create("e", "b"),
                         A2 = TestValue.Create("A ", "B"),
                       };

      var graph = new[]
                  {
                    testValues.A,
                    testValues.B,
                    testValues.C,
                    testValues.D,
                    testValues.E,
                    testValues.A2,
                  };

      var ignoreCase = DerivedComparer.Create(StringComparer.OrdinalIgnoreCase, (string t) => t.Trim());

      var components1 = graph.DetectCyclesUsingKey(t => t.Value, s => s.DependsOn,
        keyComparer: ignoreCase,
        comparer: TestValue.CreateComparer(ignoreCase));
      RunAssertions(components1);

      var byValue = graph.ToLookup(t => t.Value, ignoreCase);
      var components2 = graph.DetectCycles(s => s.DependsOn.SelectMany(d => byValue[d]), TestValue.CreateComparer(ignoreCase));
      RunAssertions(components2);



      void RunAssertions(ICollection<DependencyCycle<TestValue<string>>> components)
      {
        Assert.AreEqual(3, components.Count);
        Assert.AreEqual(2, components.IndependentComponents().Count()); // D & E are acyclic
        Assert.AreEqual(1, components.Cycles().Count());
        Assert.AreEqual(1, components.Count(c => c.Count == 3));
        Assert.AreEqual(2, components.Count(c => c.Count == 1));

        var component1 = components.First();

        Assert.IsTrue(component1.Single() ==
                      testValues.D); // first component has to be D (used by C which is in a cycle)

        var component2 = components.Skip(1).First().ToList();

        var component3 = components.Skip(2).First();

        Assert.IsTrue(component3.Single() ==
                      testValues.E); // 3rd component has to be E (requires B which is in a cycle)

        Assert.IsTrue(component2[0] == testValues.C); // first one has to be C (used by A & B)
        Assert.IsTrue(component2[1] == testValues.B); // first one has to be B (used by A)
        Assert.IsTrue(component2[2] == testValues.A); // first one has to be A (used by C, but was in the list )
      }
    }

    // This test verifies that the correct equality comparer is used to identify values
    // 
    //    B         
    //    ↓
    //  ( C <-> A )
    //    ↓     ↓
    //    D     E
    //  

    [TestMethod, Description("This test verifies that the correct equality comparer is used to identify values")]
    public void CycleWithDuplicates()
    {
      var testValues = new
                       {
                         A = TestValue.Create("a ", "e"),
                         B = TestValue.Create(" B", " c "),
                         A2 = TestValue.Create("A ", "C"),
                         C = TestValue.Create("C", "A", "D"),
                         D = TestValue.Create("d"),
                         E = TestValue.Create("E"),
                       };

      var graph = new[]
                  {
                    testValues.A,
                    testValues.B,
                    testValues.C,
                    testValues.D,
                    testValues.E,
                    testValues.A2,
                  };

      var ignoreCase = DerivedComparer.Create(StringComparer.OrdinalIgnoreCase, (string t) => t.Trim());
      var equalityComparer = TestValue.CreateComparer(ignoreCase);
      var vertexValueComparer = new VertexValueComparer<TestValue<string>>(equalityComparer);

      var byValue = graph.ToLookup(t => t.Value, ignoreCase);

      var vertices = VertexBuilder.BuildVertices(graph, s => s.DependsOn.SelectMany(d => byValue[d]), equalityComparer).ToList();
      Assert.AreEqual(6, vertices.Count, "every source element should yield a vertex");
      Assert.AreEqual(5, vertices.Distinct().Count(), "'A' and 'a ' should yield the exact same vertex");

      var finder = new StronglyConnectedComponentFinder<TestValue<string>>();

      var simpleComponents = finder.DetectCycle(vertices, vertexValueComparer);

      var components = simpleComponents.ExtractCycles(equalityComparer);

      Assert.AreEqual(4, components.Count, "It should be only 4 components");
      Assert.AreEqual(3, components.IndependentComponents().Count(), "B, D & E are acylic");
      Assert.AreEqual(1, components.Cycles().Count(), "Only 1 cycle should be found");

      Assert.AreEqual
        (2,
         components.Take(2)
                   .SelectMany(t => t)
                   .Intersect(new[] { testValues.D, testValues.E })
                   .Count(),
         "D & E should be the first components.");

      var component3 = components.Skip(2).First();
      var component4 = components.Skip(3).First();
      Assert.AreEqual(component3.Count, 2, "3rd component should be a cycle of 2");
      Assert.AreEqual
        (component3.Intersect(new[] { testValues.A, testValues.C }).Count(),
         2,
         "3rd component should be a cycle of A & C");

      Assert.AreEqual(testValues.B, component4.Single()); // 4th one has to be B (uses A)
    }
  }
}
