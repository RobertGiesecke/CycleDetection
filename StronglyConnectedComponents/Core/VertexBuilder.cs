using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace StronglyConnectedComponents.Core
{
  public static class VertexBuilder
  {
    /// <summary>
    /// Builds a sequence of <see cref="Vertex{T}">dependancy vertices</see> from a sequence of source elements.
    /// </summary>
    /// <param name="source">Required. The sequence of source elements that need to be converted into vertices.</param>
    /// <param name="dependencySelector">Required. A delegate that takes an items of <see cref="source"/> and returns a sequence of dependencies.
    /// <remarks>Can return null to indicate no dependency.</remarks></param>
    /// <param name="comparer">Required. An implementation of <see cref="IEqualityComparer{T}"/>, this is used to compare the values with their dependencies.</param>
    public static IEnumerable<Vertex<T>> BuildVertices<T>(IEnumerable<T> source, Func<T, IEnumerable<T>> dependencySelector, IEqualityComparer<T> comparer)
    {
      if (source == null) throw new ArgumentNullException("source");
      if (dependencySelector == null) throw new ArgumentNullException("dependencySelector");
      if (comparer == null) throw new ArgumentNullException("comparer");

      var builder = new VertexBuilder<T>(dependencySelector, comparer);
      return builder.BuildVertices(source);
    }
  }

  public sealed class VertexBuilder<T>
  {
    private readonly Func<T, IEnumerable<T>> _DependencySelector;

    private readonly IDictionary<T, Vertex<T>> _VertexBySource;
    // ensure to be run once per actual value
    private readonly IDictionary<T, Vertex<T>> _VertexByActualSource = new Dictionary<T, Vertex<T>>();
    private readonly IEqualityComparer<Vertex<T>> _VertexValueComparer;

    public VertexBuilder(Func<T, IEnumerable<T>> dependencySelector, IEqualityComparer<T> comparer)
    {
      if (dependencySelector == null) throw new ArgumentNullException("dependencySelector");

      _DependencySelector = dependencySelector;
      _VertexBySource = new Dictionary<T, Vertex<T>>(comparer);
      _VertexValueComparer = new VertexValueComparer<T>(comparer);
    }

    public IEnumerable<Vertex<T>> BuildVertices(IEnumerable<T> source)
    {
      foreach (var vertex in source.Select(BuildVertex))
      {
        if (vertex.Dependencies.Count > 1)
        {
          // unique values according to the comparer
          var set = new HashSet<Vertex<T>>(vertex.Dependencies, _VertexValueComparer);

          if (set.Count != vertex.Dependencies.Count)
          {
            vertex.Dependencies = set;
          }
        }
        yield return vertex;
      }
    }

    private Vertex<T> BuildVertex(T source)
    {
      Vertex<T> vertex;
      if (_VertexByActualSource.TryGetValue(source, out vertex))
        return vertex;

      if (!_VertexBySource.TryGetValue(source, out vertex))
      {
        _VertexBySource.Add(source, vertex = new Vertex<T>(source));
        _VertexByActualSource[source] = vertex;
        AddDependencies(vertex, _DependencySelector(source));
        return vertex;
      }

      if (!_VertexByActualSource.ContainsKey(source))
      {
        AddDependencies(vertex, _DependencySelector(source));
        return vertex;
      }
      return vertex;
    }

    private void AddDependencies(Vertex<T> vertex, IEnumerable<T> dependencies)
    {
      if (dependencies == null) return;

      foreach (var d in dependencies)
        vertex.Dependencies.Add(BuildVertex(d));
    }
  }
}