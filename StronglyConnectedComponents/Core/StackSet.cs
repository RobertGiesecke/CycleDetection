using System;
using System.Collections;
using System.Collections.Generic;

namespace StronglyConnectedComponents.Core
{
	/// <summary>
	/// A wrapper around a Stack<T> which uses a <see cref="IEqualityComparer{T}"/> and a <see cref="HashSet{T}"/> to make value identity configurable.
	/// </summary>
	internal class StackSet<T> : ICollection<T>
	{
		private readonly HashSet<T> _Set;
		private readonly Stack<T> _Stack;

		public StackSet(IEqualityComparer<T> comparer = null)
		{
			_Stack = new Stack<T>();
			_Set = comparer != null ? new HashSet<T>(comparer) : new HashSet<T>();
		}

		public IEqualityComparer<T> Comparer
		{
			get { return _Set.Comparer; }
		}

		void ICollection<T>.Add(T item)
		{
			_Stack.Push(item);
		}

		public void Clear()
		{
			_Stack.Clear();
		}

		public bool Contains(T item)
		{
			return _Set.Contains(item);
		}


		void ICollection<T>.CopyTo(T[] array, int arrayIndex)
		{
			_Stack.CopyTo(array, arrayIndex);
		}

		bool ICollection<T>.Remove(T item)
		{
			throw new NotImplementedException();
		}

		public int Count
		{
			get { return _Stack.Count; }
		}

		bool ICollection<T>.IsReadOnly { get { return false; } }

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _Stack.GetEnumerator();
		}

		public T Peek()
		{
			return _Stack.Peek();
		}

		public T Pop()
		{
			var popped = _Stack.Pop();
			_Set.Remove(popped);
			return popped;
		}

		public void Push(T item)
		{
			if (!_Set.Add(item))
				throw new ArgumentException("An identical item has already been pushed.", "item");
			_Stack.Push(item);
		}

		public T[] ToArray()
		{
			return _Stack.ToArray();
		}

		public void TrimExcess()
		{
			_Stack.TrimExcess();
			_Set.TrimExcess();
		}
	}
}