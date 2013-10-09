using System;
using System.Collections.Generic;

namespace StronglyConnectedComponents
{
  static internal class DependencyVisitor
  {
    public delegate DependencyCycle<T> VisitCycleHandler<T>(DependencyCycle<T> cycle, bool dependenciesChanged, ISet<DependencyCycle<T>> dependencies);

    public static IEnumerable<DependencyCycle<T>> VisitCycles<T>(this IEnumerable<DependencyCycle<T>> cycles, VisitCycleHandler<T> visit)
    {
      var mc = new Dictionary<DependencyCycle<T>, DependencyCycle<T>>();

      var setComparer = HashSet<DependencyCycle<T>>.CreateSetComparer();
      return VisitCyclesCore(cycles, () => { }, mc, visit, setComparer);
    }

    private static IEnumerable<DependencyCycle<T>> VisitCyclesCore<T>(IEnumerable<DependencyCycle<T>> cycles, Action cancelled, IDictionary<DependencyCycle<T>, DependencyCycle<T>> cache, DependencyVisitor.VisitCycleHandler<T> visit, IEqualityComparer<HashSet<DependencyCycle<T>>> setComparer)
    {
      foreach (var cycle in cycles)
      {
        DependencyCycle<T> newCycle;
        if (cache.TryGetValue(cycle, out newCycle))
        {
          yield return cycle;
          continue;
        }

        var dependenciesChanged = false;

        var dependencies = cycle.Dependencies as HashSet<DependencyCycle<T>> ?? cycle.Dependencies.ToHashSet();

        if (dependencies.Count > 0)
        {
          var wasCancelled = false;
          var dependenciesSet = VisitCyclesCore(dependencies, () => wasCancelled = true, cache, visit, setComparer).ToHashSet();

          if (wasCancelled)
          {
            cancelled();
            yield break;
          }

          if (!setComparer.Equals(dependenciesSet, dependencies))
          {
            dependenciesChanged = true;
            dependencies = dependenciesSet;
          }
        }

        newCycle = visit(cycle, dependenciesChanged, dependencies);
        if (newCycle == null)
        {
          cancelled();
          yield break;
        }

        if (newCycle == cycle && dependenciesChanged)
        {
          newCycle = new DependencyCycle<T>(cycle.Contents, dependencies);
        }

        cache.Add(cycle, newCycle);
        yield return newCycle;
      }
    }
  }
}