using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StronglyConnectedComponents.Tests.Utils;

namespace StronglyConnectedComponents.Tests
{
  [TestClass]
  public class DependencyCycleMergerTests : TestBase
  {
    // A→B
    // ↑ ↓
    // └─C-→D
    [TestMethod, ExpectedException(typeof(CyclicDependenciesDetectedException))]
    public void MergeCycleWithoutMergeThrows()
    {
      TestValue<string> vD;
      var graph = new[]
                  {
                    TestValue.Create("A", "B"),
                    TestValue.Create("B", "C"),
                    TestValue.Create("C", "A", "D"),
                    vD = TestValue.Create("D"),
                  };
      var byValue = graph.ToLookup(t => t.Value);
      var components = graph.DetectCycles(s => s.DependsOn.SelectMany(d => byValue[d]));
      Assert.AreNotEqual(components.Cycles().Count(), 0);
      var flattened = components.MergeCyclicDependencies().ToList();
    }

    //  ┌─→ B ← E       
    //  |   ↓   ↓
    //  A ↔ C   X
    //      ↓
    //      D
    //  
    [TestMethod]
    public void MergeCycleWithMergeDoesNotThrows()
    {
      var graph = new[]
                  {
                    TestValue.Create("A", "B"),
                    TestValue.Create("B", "C"),
                    TestValue.Create("C", "A", "D"),
                    TestValue.Create("D"),
                    TestValue.Create("E", "B", "X"),
                    TestValue.Create("X")
                  };

      var byValue = graph.ToLookup(t => t.Value);
      var components = graph.DetectCycles(s => s.DependsOn.SelectMany(d => byValue[d]));

      Assert.AreNotEqual(components.Cycles().Count(), 0);

      var flattened = components.MergeCyclicDependencies
        ((cycle, getFlattened) =>
          // create a new TestValue<> by joining all cyclic values & dependencies
         TestValue.Create
           (Join
              ("-",
               from v in cycle.Contents
               orderby v.Value
               select v.Value),
            (from t in cycle.Dependencies
             let merged = getFlattened(t)
             select merged != null
                      ? merged.Value
                      : t.Contents.Single().Value).ToArray())).ToList();

      Assert.AreEqual("D", flattened[0].Value);
      Assert.AreEqual("A-B-C", flattened[1].Value);
      Assert.AreEqual("X", flattened[2].Value);
      Assert.AreEqual("E", flattened[3].Value);

      Assert.IsTrue(flattened[1].DependsOn.Contains("D"), "A-B-C should have D as dependency");

      Assert.IsTrue(flattened[3].DependsOn.Contains("A-B-C"), "E should have A-B-C as dependency");
      Assert.IsTrue(flattened[3].DependsOn.Contains("X"), "E should have X as dependency");
    }
  }
}