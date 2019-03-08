using System.Collections.Generic;

namespace StronglyConnectedComponents.Core
{
    public class StronglyConnectedComponent<T> : IEnumerable<Vertex<T>>
    {
        private LinkedList<Vertex<T>> list;

        public StronglyConnectedComponent()
        {
            list = new LinkedList<Vertex<T>>();
        }

        public StronglyConnectedComponent(IEnumerable<Vertex<T>> collection)
        {
            list = new LinkedList<Vertex<T>>(collection);
        }

        public void Add(Vertex<T> vertex)
        {
            list.AddLast(vertex);
        }

        public IEnumerator<Vertex<T>> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public int Count => list.Count;

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public bool IsCycle => list.Count > 1;
    }
}
