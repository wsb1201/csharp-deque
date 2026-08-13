namespace DequeCollection.Core;

using System.Diagnostics;

internal sealed class RingBuffer<T>
{
	// Invariant: _head < Capacity, unless both are zero.
	private WrappedIndex _head = new(0);

	// The number of initialized elements, starting from the one at `_head` and potentially wrapping around.
	// When _count is zero, the value of _head is meaningless.
	private int _count = 0;

	private T[] _buffer = [];

	internal int Count
	{
		get => _count;
		set => _count = value;
	}
	internal WrappedIndex Head
	{
		get => _head;
		set => _head = value;
	}
	internal int Capacity => _buffer.Length;

	internal T[] Buffer => _buffer;

	internal T this[WrappedIndex index]
	{
		get => _buffer[index.Index];
		set => _buffer[index.Index] = value;
	}

	internal RingBuffer(int capacity)
	{
		if (int.IsNegative(capacity))
			throw new ArgumentOutOfRangeException(
				nameof(capacity),
				"Capacity must be non-negative."
			);
		Reserve(capacity);
	}

	internal void Reserve(int additional, bool exact = false)
	{
		var newCap = checked(_count + additional);
		var oldCap = Capacity;
		var oldHead = _head;

		if (newCap > oldCap)
		{
			if (!exact)
				newCap = int.Max(4, checked(int.Max(newCap, oldCap * 2)));

			var contiguous = IsContiguous();
			Array.Resize(ref _buffer, newCap);
			if (contiguous)
				return;

			// Move the shortest contiguous section of the ring buffer
			var headLen = oldCap - oldHead.Index;
			var tailLen = _count - headLen;
			if (headLen > tailLen && newCap - oldCap >= tailLen)
				MoveSegment(0, oldCap, headLen);
			else
			{
				_head = new WrappedIndex(newCap - headLen);
				Debug.Assert(_head.Index < newCap);
				MoveSegment(oldHead.Index, _head.Index, headLen);
			}
		}
	}

	private void MoveSegment(int sourceIndex, int destinationIndex, int length)
	{
		Array.Copy(_buffer, sourceIndex, _buffer, destinationIndex, length);
		Array.Clear(_buffer, sourceIndex, length);
	}

	// Double the buffer size if full.
	internal void GrowOneAmortized()
	{
		if (IsFull())
			Reserve(additional: 1, exact: false);
	}

	// Returns `true` if the buffer is at full capacity.
	internal bool IsFull() => _count == Capacity;

	internal bool IsContiguous() => _head.Index <= Capacity - _count;

	// If the buffer is empty, this method throws an InvalidOperationException.
	internal void AssertNotEmpty()
	{
		if (_count == 0)
			throw new InvalidOperationException("Queue empty.");
	}

	// Given a range into the logical ring buffer, this function return two
	// ranges into the physical buffer that correspond to the given range.
	//
	// For the resulting ranges to be valid ranges into the physical buffer,
	// the caller must ensure the given range represents a valid range into
	// the logical buffer, and that all elements in that range are initialized.
	internal (Range front, Range back) SegmentRanges(Range range)
	{
		Debug.Assert(!int.IsNegative(_count));
		int start = range.Start.GetOffset(_count);
		int end = range.End.GetOffset(_count);

		if ((uint)start > (uint)_count)
			throw new ArgumentOutOfRangeException(
				nameof(range),
				$"Start index {start} out of range for length {_count}."
			);
		if ((uint)end > (uint)_count)
			throw new ArgumentOutOfRangeException(
				nameof(range),
				$"End index {end} out of range for length {_count}."
			);
		if (start > end)
			throw new ArgumentException(
				$"Invalid range: start {start} must be <= end {end}.",
				nameof(range)
			);

		var len = end - start;

		if (len == 0)
			return (0..0, 0..0);

		var wrapped_start = WrapIndex(unchecked(_head.Index + start));

		var headLen = Capacity - wrapped_start.Index;
		if (headLen >= len)
			return (wrapped_start.Index..(wrapped_start.Index + len), 0..0);

		var tailLen = len - headLen;
		return (wrapped_start.Index..Capacity, 0..tailLen);
	}

	// Deque must not be modified while the enumerator is in use.
	internal IEnumerable<T> TrustedEnumerator()
	{
		for (var (index, count) = (_head.Index, _count); count-- > 0; index++)
			yield return this[GetIndex(index)];
	}

	internal WrappedIndex GetIndex(int index) =>
		index >= 0 && index < _count
			? WrapIndex(unchecked(_head.Index + index))
			: throw new ArgumentOutOfRangeException(
				nameof(index),
				$"Index {index} out of range for length {_count}."
			);

	internal WrappedIndex FrontIndex()
	{
		AssertNotEmpty();
		return WrapIndex(_head.Index);
	}

	internal WrappedIndex BackIndex()
	{
		AssertNotEmpty();
		return WrapIndex(unchecked(_head.Index + _count - 1));
	}

	// Returns the index in the underlying buffer for a given logical element index.
	internal WrappedIndex WrapIndex(int logical_index)
	{
		Debug.Assert(
			(logical_index == 0 && Capacity == 0)
				|| logical_index < Capacity
				|| (logical_index - Capacity) < Capacity
		);
		return logical_index >= Capacity
			? new WrappedIndex(logical_index - Capacity)
			: new WrappedIndex(logical_index);
	}
}

// Represents an index that can be used to index the ring buffer.
// It is meant to help avoid passing logical (unwrapped) indices to the
// underlying buffer by accident.
//
// The invariant of this index is that it is always < buffer capacity, unless
// the buffer is empty (in that case the index can be 0 when capacity is 0).
// The index is always non-negative.
internal readonly record struct WrappedIndex
{
	internal readonly int Index { get; private init; }

	// Invariant: the newly constructed index must remain in-bounds for the ring buffer.
	internal WrappedIndex(int index)
	{
		if (int.IsNegative(index))
			throw new ArgumentOutOfRangeException(nameof(index), "Non-negative number required.");
		Index = index;
	}
}
