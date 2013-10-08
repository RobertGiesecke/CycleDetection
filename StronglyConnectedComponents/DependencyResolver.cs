using System;
using System.Collections.Generic;
using System.Linq;
using StronglyConnectedComponents.Core;

namespace StronglyConnectedComponents
{
  public static class DependencyResolver
  {
    public static IEnumerable<T> FlattenCyclicDependencies<T>(this IEnumerable<DependencyCycle<T>> components, Func<DependencyCycle<T>, T> mergeCycle = null)
    {
      if (mergeCycle == null)
        mergeCycle = set => { throw new InvalidOperationException(string.Format("A cyclic dependency has been detected. This method cannot continue without a value for mergeCycle.")); };

      return from c in components
             select c.IsCyclic ? mergeCycle(c) : c.Contents.Single();
    }

    public static IList<DependencyCycle<T>> DetectCycles<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> dependencySelector, IEqualityComparer<T> comparer = null)
    {
      var valueComparer = comparer ?? EqualityComparer<T>.Default;

      var components = ResolveStronglyConnectedComponents(source, dependencySelector, valueComparer);

      return (from c in components
              let cycleSet = new HashSet<T>(c.Select(t => t.Value), valueComparer)
              select new DependencyCycle<T>(cycleSet.Count > 1,
                                            cycleSet,
                                            new HashSet<T>(c.SelectMany(v => v.Dependencies.Select(dv => dv.Value)), valueComparer))).ToList();
    }

    public static StronglyConnectedComponentList<T> ResolveStronglyConnectedComponents<T>(IEnumerable<T> source, Func<T, IEnumerable<T>> dependencySelector, IEqualityComparer<T> comparer = null)
    {
      var valueComparer = comparer ?? EqualityComparer<T>.Default;
      var vertexComparer = new VertexValueComparer<T>(valueComparer);

      var finder = new StronglyConnectedComponentFinder<T>();

      var vertices = source.BuildVertices(dependencySelector, valueComparer);

      var result = finder.DetectCycle(vertices, vertexComparer);
      return result;
    }

    public static IEnumerable<DependencyCycle<T>> IndependentComponents<T>(this IEnumerable<DependencyCycle<T>> components)
    {
      if (components == null) throw new ArgumentNullException("components");
      return components.Where(c => !c.IsCyclic);
    }

    public static IEnumerable<DependencyCycle<T>> Cycles<T>(this IEnumerable<DependencyCycle<T>> components)
    {
      if (components == null) throw new ArgumentNullException("components");
      return components.Where(c => c.IsCyclic);
    }
  }
}