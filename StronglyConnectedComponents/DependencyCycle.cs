using System.Collections;
using System.Collections.Generic;

namespace StronglyConnectedComponents
{
  public sealed class DependencyCycle<T> : IEnumerable<T>
  {
    public bool IsCyclic { get; private set; }
    public ISet<T> Contents { get; private set; }
    public ISet<T> Dependencies { get; private set; }

    public int Count
    {
      get { return Contents.Count; }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerator<T> GetEnumerator()
    {
      return Contents.GetEnumerator();
    }

    public DependencyCycle(bool isCyclic, ISet<T> contents, ISet<T> dependencies)
    {
      IsCyclic = isCyclic;
      Contents = contents;
      Dependencies = dependencies;
    }
  }
}