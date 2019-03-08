using System.Collections.Generic;
using System.Linq;

namespace StronglyConnectedComponents.Core
{
    public class StronglyConnectedComponentList<T> : IEnumerable<StronglyConnectedComponent<T>>
    {
        private LinkedList<StronglyConnectedComponent<T>> collection;

        public StronglyConnectedComponentList()
        {
            collection = new LinkedList<StronglyConnectedComponent<T>>();
        }

        public StronglyConnectedComponentList(IEnumerable<StronglyConnectedComponent<T>> collection)
        {
            this.collection = new LinkedList<StronglyConnectedComponent<T>>(collection);
        }

        public void Add(StronglyConnectedComponent<T> scc)
        {
            collection.AddLast(scc);
        }

        public int Count => collection.Count;

        public IEnumerator<StronglyConnectedComponent<T>> GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        public IEnumerable<StronglyConnectedComponent<T>> IndependentComponents()
        {
            return this.Where(c => !c.IsCycle);
        }

        public IEnumerable<StronglyConnectedComponent<T>> Cycles()
        {
            return this.Where(c => c.IsCycle);
        }
    }
}
