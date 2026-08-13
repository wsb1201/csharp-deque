namespace DequeCollection.Core;

using System.Collections;
using System.Diagnostics;

// A double-ended queue implemented with a growable ring buffer.
public class Deque<T> : IDeque<T>
{
	private readonly RingBuffer<T> _ring;
	private int _version;

	// Creates an empty Deque.
	public Deque() => _ring = new(capacity: 0);

	// Creates an empty Deque with space for at least capacity elements.
	public Deque(int capacity) => _ring = new(capacity);

	// Fills a new Deque<T> with the elements of another Deque<T>.
	public Deque(Deque<T> source)
		: this(source._ring.TrustedEnumerator(), sizeHint: source.Count) { }

	// Fills a new Deque<T> with the elements of an ICollection<T>.
	// Uses the enumerator to get each of the elements.
	public Deque(ICollection<T> source)
		: this(source.AsEnumerable(), sizeHint: source.Count) { }

	// Fills a new Deque<T> with the elements of an IEnumerable<T>.
	public Deque(IEnumerable<T> source, int sizeHint = 0)
		: this(sizeHint)
	{
		foreach (var item in source)
			PushBack(item);
	}

	public T this[int index]
	{
		get => _ring[_ring.GetIndex(index)];
		set => _ring[_ring.GetIndex(index)] = value;
	}

	// Returns the number of elements contained in the Deque<T>.
	public int Count => _ring.Count;

	// Returns the number of elements the Deque<T> can hold without reallocating.
	public int Capacity => _ring.Capacity;

	// Returns a value indicating whether the Deque<T> is at full capacity.
	public bool IsFull => _ring.IsFull();

	// Returns a value indicating whether the Deque<T> is empty.
	public bool IsEmpty => Count == 0;

	// Returns a value indicating whether the Deque<T> is contiguous in memory.
	public bool IsContiguous => _ring.IsContiguous();

	// Returns a pair of spans which contain, in order, the contents of the Deque<T>.
	public Slices<T> AsSlices() => AsSlices(..);

	public Slices<T> AsSlices(Range range)
	{
		var (front, back) = _ring.SegmentRanges(range);
		return new(_ring.Buffer.AsSpan(front), _ring.Buffer.AsSpan(back));
	}

	// Reserves space for at least the given number of additional elements.
	public void Reserve(int additional)
	{
		_ring.Reserve(additional, exact: false);
		_version++;
	}

	// Reserves space for exactly the given number of additional elements.
	// Prefer reserve if future insertions are expected.
	public void ReserveExact(int additional)
	{
		_ring.Reserve(additional, exact: true);
		_version++;
	}

	// Removes all elements from the Deque<T>.
	public void Clear()
	{
		var (front, back) = AsSlices();
		front.Clear();
		back.Clear();
		_ring.Count = 0;
		_ring.Head = new(0);
		_version++;
	}

	// CopyTo copies the elements of the deque into an array, starting at a
	// particular index into the array.
	public void CopyTo(T[] array, int index)
	{
		var (front, back) = AsSlices();
		var dest = array.AsSpan(index, _ring.Count);
		front.CopyTo(dest[..front.Length]);
		back.CopyTo(dest.Slice(front.Length, back.Length));
	}

	// Returns an array of the elements in the Deque<T>, or an empty array if the Deque<T> is empty.
	public T[] ToArray() => ToArray(..);

	// Returns an array of the elements within the given range of the Deque<T>,
	// or an empty array if either the Deque<T> or the range is empty.
	public T[] ToArray(Range range)
	{
		var slices = AsSlices(range);
		var array = new T[slices.Length];
		slices.Front.CopyTo(array.AsSpan(0, slices.Front.Length));
		slices.Back.CopyTo(array.AsSpan(slices.Front.Length, slices.Back.Length));
		return array;
	}

	// Prepends an element to the deque.
	public void PushFront(T value)
	{
		_ring.GrowOneAmortized();
		_ring.Count++;
		_ring.Head = _ring.WrapIndex(unchecked(_ring.Head.Index - 1 + _ring.Capacity));
		_ring[_ring.Head] = value;
		_version++;
	}

	// Appends an element to the deque.
	public void PushBack(T value)
	{
		_ring.GrowOneAmortized();
		var count = _ring.Count++;
		var index = _ring.WrapIndex(unchecked(_ring.Head.Index + count));
		_ring[index] = value;
		_version++;
	}

	// Removes the first element and returns it. Throws
	// InvalidOperationException if the deque is empty.
	public T PopFront()
	{
		_ring.AssertNotEmpty();
		var index = _ring.Head;
		_ring.Head = _ring.WrapIndex(unchecked(_ring.Head.Index + 1));
		_ring.Count--;
		Debug.Assert(_ring.Count < _ring.Capacity);

		var value = _ring[index];
		_ring[index] = default!;
		_version++;
		return value;
	}

	// Removes the last element and returns it. Throws
	// InvalidOperationException if the deque is empty.
	public T PopBack()
	{
		_ring.AssertNotEmpty();
		_ring.Count--;
		Debug.Assert(_ring.Count < _ring.Capacity);
		var index = _ring.WrapIndex(unchecked(_ring.Head.Index + _ring.Count));

		var value = _ring[index];
		_ring[index] = default!;
		_version++;
		return value;
	}

	// Returns the first element without removing it from the deque.
	// Throws InvalidOperationException if the deque is empty.
	public T PeekFront()
	{
		_ring.AssertNotEmpty();
		return _ring[_ring.FrontIndex()];
	}

	// Returns the last element without removing it from the deque.
	// Throws InvalidOperationException if the deque is empty.
	public T PeekBack()
	{
		_ring.AssertNotEmpty();
		return _ring[_ring.BackIndex()];
	}

	public bool TryPopFront(out T value)
	{
		if (IsEmpty)
		{
			value = default!;
			return false;
		}
		else
		{
			value = PopFront();
			return true;
		}
	}

	public bool TryPopBack(out T value)
	{
		if (IsEmpty)
		{
			value = default!;
			return false;
		}
		else
		{
			value = PopBack();
			return true;
		}
	}

	// Returns true if the Deque contains an element equal to the given item.
	// Equality is determined using the default equality comparer.
	public bool Contains(T item) => Enumerable.Contains(_ring.TrustedEnumerator(), item);

	// Returns true if the Deque contains an element equal to the given item.
	public bool Contains(T item, IEqualityComparer<T>? comparer) =>
		Enumerable.Contains(_ring.TrustedEnumerator(), item, comparer);

	public IEnumerator<T> GetEnumerator() => new Enumerator(this);

	IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

	private sealed class Enumerator(Deque<T> q) : IEnumerator<T>
	{
		private IEnumerator<T>? _inner = null;
		private bool finished = false;
		private readonly int _version = q._version;

		private void AssertUnchanged()
		{
			if (_version != q._version)
				throw new InvalidOperationException(
					"Deque was modified; enumeration operation may not execute."
				);
		}

		object IEnumerator.Current => Current!;

		public T Current =>
			finished ? throw new InvalidOperationException("Enumeration already finished.")
			: _inner != null ? _inner.Current
			: throw new InvalidOperationException("Enumeration has not started. Call MoveNext.");

		bool IEnumerator.MoveNext()
		{
			AssertUnchanged();
			_inner ??= q._ring.TrustedEnumerator().GetEnumerator();
			finished = finished || !_inner.MoveNext();
			return !finished;
		}

		void IEnumerator.Reset()
		{
			AssertUnchanged();
			_inner = null;
			finished = false;
		}

		public void Dispose() { }
	}
}

public readonly ref struct Slices<T>
{
	public readonly Span<T> Front { get; internal init; }
	public readonly Span<T> Back { get; internal init; }

	public readonly int Length => checked(Front.Length + Back.Length);

	// Returns a value that indicates whether the slices are empty.
	public readonly bool IsEmpty => Front.IsEmpty;

	// Returns a value that indicates whether the back slice is empty.
	public readonly bool IsContiguous => Back.IsEmpty;

	public void Deconstruct(out Span<T> front, out Span<T> back)
	{
		front = Front;
		back = Back;
	}

	internal Slices(Span<T> front, Span<T> back)
	{
		Debug.Assert(!front.IsEmpty || back.IsEmpty);
		Front = front;
		Back = back;
	}
}
