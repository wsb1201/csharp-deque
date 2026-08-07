using System.Diagnostics;

namespace DequeCollection.Core;

public partial class Deque<T>
{
	private T?[] _data = [];
	private int _head;

	public int Count { get; private set; }
	public int Capacity => _data.Length;
}

public partial class Deque<T>
{
	public T this[int idx]
	{
		get => _data[WrapAdd(IndexRangeAssert(idx))]!;
		set => _data[WrapAdd(IndexRangeAssert(idx))] = value;
	}

	private int IndexRangeAssert(int idx) =>
		(idx >= 0 && idx < Count) ? idx : throw new IndexOutOfRangeException();

	/// Returns `true` if the deque is at full capacity.
	public bool IsFull() => Count == Capacity;

	/// Returns `true` if the deque is empty.
	public bool IsEmpty() => Count == 0;

	/// Returns `true` if the deque is contiguous.
	public bool IsContiguous() => _head + Count <= Capacity;
}

public partial class Deque<T>
{
	/// Appends an element to the deque.
	public void PushBack(T value)
	{
		if (IsFull())
			Grow();

		var len = Count++;
		_data[WrapAdd(len)] = value;
	}

	/// Prepends an element to the deque.
	public void PushFront(T value)
	{
		if (IsFull())
			Grow();

		_head = WrapSub(1);
		Count++;
		_data[_head] = value;
	}

	/// Removes the first element and returns it. Throws
	/// InvalidOperationException if the deque is empty.
	public T PopFront() => TryPopFront(out T value) ? value : throw new InvalidOperationException();

	/// Removes the last element and returns it. Throws
	/// InvalidOperationException if the deque is empty.
	public T PopBack() => TryPopBack(out T value) ? value : throw new InvalidOperationException();

	public bool TryPopFront(out T value)
	{
		if (IsEmpty())
		{
			value = default!;
			return false;
		}

		Debug.Assert(Count <= Capacity);
		var oldHead = _head;
		_head = WrapAdd(1);
		Count--;
		value = _data[oldHead]!;
		_data[oldHead] = default;
		return true;
	}

	public bool TryPopBack(out T value)
	{
		if (IsEmpty())
		{
			value = default!;
			return false;
		}

		Debug.Assert(Count <= Capacity);
		var idx = WrapAdd(--Count);
		value = _data[idx]!;
		_data[idx] = default;
		return true;
	}
}

partial class Deque<T>
{
	private int WrapAdd(int add) => WrapIndex(_head + add);

	private int WrapSub(int sub) => WrapIndex(_head - sub + Capacity);

	/// Returns the index in the underlying buffer for a given logical element index.
	private int WrapIndex(int idx)
	{
		Debug.Assert((idx == 0 && Capacity == 0) || idx < Capacity || (idx - Capacity) < Capacity);
		return (idx >= Capacity) ? idx - Capacity : idx;
	}

	// Double the ring buffer size.
	private void Grow()
	{
		if (!IsFull())
			return;
		else if (Capacity >= 1 << 30) // overflow guard
			throw new OutOfMemoryException();

		var oldCapacity = Capacity;
		var newCapacity = int.Max(4, oldCapacity << 1);
		Debug.Assert(newCapacity >= oldCapacity);

		// !! call before `_data` is updated !!
		var contiguous = IsContiguous();

		var old = _data;
		_data = new T[newCapacity];

		if (Count > 0)
		{
			void MoveToNew(int src, int dst, int count)
			{
				Debug.Assert(src + count <= oldCapacity);
				Debug.Assert(dst + count <= newCapacity);
				Array.Copy(old, src, _data, dst, count);
			}

			if (contiguous)
			{
				MoveToNew(_head, _head, Count);
			}
			else
			{ // Move the shortest contiguous section of the ring buffer.
				var headCount = oldCapacity - _head;
				var tailCount = Count - headCount;
				if (headCount > tailCount && newCapacity - oldCapacity >= tailCount)
				{
					MoveToNew(0, oldCapacity, tailCount);
				}
				else
				{
					var newHead = newCapacity - headCount;
					MoveToNew(_head, newHead, headCount);
					_head = newHead;
				}
			}
		}

		Debug.Assert(_head < Capacity || Capacity == 0);
		Debug.Assert(!IsFull());
	}
}
