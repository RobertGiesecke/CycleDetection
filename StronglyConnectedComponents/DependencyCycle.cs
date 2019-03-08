using System.Collections;
using System.Collections.Generic;

namespace StronglyConnectedComponents
{
  public sealed class DependencyCycle<T> : IEnumerable<T>
  {
    public bool IsCyclic { get; }
    public ISet<T> Contents { get; }
    public ISet<DependencyCycle<T>> Dependencies { get; }

    public int Count => Contents.Count;

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerator<T> GetEnumerator()
    {
      return Contents.GetEnumerator();
    }

    public DependencyCycle(ISet<T> contents, ISet<DependencyCycle<T>> dependencies)
    {
      IsCyclic = contents.Count > 1;
      Contents = contents;
      Dependencies = dependencies;
    }
  }
}