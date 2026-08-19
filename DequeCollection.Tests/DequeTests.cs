using DequeCollection.Core;

namespace DequeCollection.Tests;

public sealed class DequeTests
{
	[Fact]
	public void DefaultCtor_HasExpectedInitialState()
	{
		// Given
		var d = new Deque<int>();

		// Then
		Assert.Equal(0, d.Count);
		Assert.Equal(0, d.Capacity);
		Assert.True(d.IsFull); // IsFull is true for a deque with zero capacity
		Assert.True(d.IsEmpty);
		Assert.True(d.IsContiguous);
	}

	[Theory]
	[InlineData(7)]
	[InlineData(8)]
	[InlineData(10)]
	[InlineData(24)]
	[InlineData(44)]
	public void WithCapacityCtor_HasExpectedInitialState(int capacity)
	{
		// Given
		var d = new Deque<int>(capacity);

		// Then
		Assert.Equal(0, d.Count);
		Assert.True(d.Capacity >= capacity);
		Assert.False(d.IsFull);
		Assert.True(d.IsEmpty);
		Assert.True(d.IsContiguous);
	}

	[Fact]
	public void PushFront_PopFront_LIFO()
	{
		// Given
		var d = new Deque<int>();

		// When
		d.PushFront(1);
		d.PushFront(2);
		d.PushFront(3);

		// Then
		Assert.Equal(3, d.PopFront());
		Assert.Equal(2, d.PopFront());
		Assert.Equal(1, d.PopFront());

		Assert.Equal(0, d.Count);
		Assert.True(d.IsEmpty);
	}

	[Fact]
	public void PushBack_PopBack_LIFO()
	{
		// Given
		var d = new Deque<string>();

		// When
		d.PushBack("alpha");
		d.PushBack("beta");
		d.PushBack("gamma");
		d.PushBack("delta");

		// Then
		Assert.Equal("delta", d.PopBack());
		Assert.Equal("gamma", d.PopBack());
		Assert.Equal("beta", d.PopBack());
		Assert.Equal("alpha", d.PopBack());

		Assert.Equal(0, d.Count);
		Assert.True(d.IsEmpty);
	}

	[Fact]
	public void PushFront_PopBack_FIFO()
	{
		// Given
		var d = new Deque<int>();

		// When
		d.PushFront(1);
		d.PushFront(1);
		d.PushFront(2);
		d.PushFront(2);
		d.PushFront(3);
		d.PushFront(6);
		d.PushFront(12);
		d.PushFront(13);

		// Then
		Assert.Equal(1, d.PopBack());
		Assert.Equal(1, d.PopBack());
		Assert.Equal(2, d.PopBack());
		Assert.Equal(2, d.PopBack());
		Assert.Equal(3, d.PopBack());
		Assert.Equal(6, d.PopBack());
		Assert.Equal(12, d.PopBack());
		Assert.Equal(13, d.PopBack());
	}

	[Fact]
	public void PushFront_PushBack_MaintainsLogicalOrder()
	{
		// Given
		var d = new Deque<char>();

		// When
		d.PushBack('c');
		d.PushFront('b');
		d.PushBack('d');
		d.PushBack('e');
		d.PushFront('a');

		// Then
		Assert.Equal(['a', 'b', 'c', 'd', 'e'], [.. d]);
		Assert.Equal('a', d.PeekFront());
		Assert.Equal('e', d.PeekBack());
	}

	[Fact]
	public void PopFront_PopBack_ThrowsWhenEmpty()
	{
		// Given
		var d = new Deque<int>();

		// Then
		Assert.Throws<InvalidOperationException>(() => d.PopFront());
		Assert.Throws<InvalidOperationException>(() => d.PopBack());
	}

	[Fact]
	public void TryPopFront_TryPopBack_FalseWhenEmpty()
	{
		// Given
		var d = new Deque<int>();

		// When
		var okFront = d.TryPopFront(out var valueFront);
		var okBack = d.TryPopBack(out var valueBack);

		// Then
		Assert.False(okFront);
		Assert.False(okBack);
		Assert.Equal(default, valueFront);
		Assert.Equal(default, valueBack);
		Assert.True(d.IsEmpty);
	}

	[Fact]
	public void TryPopFront_TryPopBack_TrueWhenHasItems()
	{
		// Given
		var d = new Deque<string>();

		// When
		d.PushBack("back");
		d.PushFront("front");
		var okFront = d.TryPopFront(out var valueFront);
		var okBack = d.TryPopBack(out var valueBack);

		// Then
		Assert.True(okFront);
		Assert.True(okBack);
		Assert.Equal("front", valueFront);
		Assert.Equal("back", valueBack);
		Assert.True(d.IsEmpty);
	}

	[Fact]
	public void PopFront_RemovesFromFrontAndUpdatesCountAndOrder()
	{
		// Given
		var d = new Deque<int>(3);

		// When
		d.PushBack(10);
		d.PushBack(20);
		d.PushBack(30);

		// Then
		Assert.Equal(10, d.PopFront());
		Assert.Equal(2, d.Count);
		Assert.Equal([20, 30], [.. d]);

		Assert.Equal(20, d.PopFront());
		Assert.Equal(1, d.Count);
		Assert.Equal([30], [.. d]);

		Assert.Equal(30, d.PopFront());
		Assert.Equal(0, d.Count);
	}

	// Invariant: PopBack updates Count/ordering correctly for a multi-element deque.
	[Fact]
	public void PopBack_RemovesFromBackAndUpdatesCountAndOrder()
	{
		// Given
		var d = new Deque<int>(3);

		// When
		d.PushBack(10);
		d.PushBack(20);
		d.PushBack(30);

		// Then
		Assert.Equal(30, d.PopBack());
		Assert.Equal(2, d.Count);
		Assert.Equal([10, 20], [.. d]);

		Assert.Equal(20, d.PopBack());
		Assert.Equal(1, d.Count);
		Assert.Equal([10], [.. d]);

		Assert.Equal(10, d.PopBack());
		Assert.Equal(0, d.Count);
	}

	[Fact]
	public void Enumerator_IteratesOverLogicalOrder()
	{
		// Given
		var d = new Deque<int>();

		// When
		d.PushBack(1);
		d.PushBack(2);
		d.PushFront(0);

		// Then
		Assert.Equal([0, 1, 2], [.. d]);
	}

	[Fact]
	public void Enumerator_ThrowsWhenModified()
	{
		// Given
		var d = new Deque<int>();

		// When
		d.PushBack(1);
		d.PushBack(2);
		d.PushBack(3);

		// Then
		using var e = d.GetEnumerator();
		Assert.True(e.MoveNext());
		Assert.Equal(1, e.Current);

		d.PushBack(4);
		Assert.Throws<InvalidOperationException>(() => e.MoveNext());
	}

	[Fact]
	public void Enumerator_CurrentBeforeStart_Throws()
	{
		// Given
		var d = new Deque<int>();

		// When
		d.PushBack(0);

		// Then
		using var e = d.GetEnumerator();
		Assert.Throws<InvalidOperationException>(() => _ = e.Current);
	}

	[Fact]
	public void Enumerator_CurrentAfterFinish_Throws()
	{
		// Given
		var d = new Deque<int>();
		using var e = d.GetEnumerator();

		// Then
		Assert.False(e.MoveNext());
		Assert.Throws<InvalidOperationException>(() => _ = e.Current);
	}

	[Fact]
	public void CopyCtor_CopiesInOrder()
	{
		// Given
		var src = new Deque<int>();

		// When
		src.PushBack(1);
		src.PushBack(2);
		src.PushFront(0);

		// Then
		var copy = new Deque<int>(src);
		Assert.Equal([0, 1, 2], [.. copy]);
		Assert.Equal(src.Count, copy.Count);
	}

	[Fact]
	public void ICollectionCtor_CopiesInOrder()
	{
		// Given
		ICollection<int> src = [1, 2, 3];

		// When
		var d = new Deque<int>(src);

		// Then
		Assert.Equal([1, 2, 3], [.. d]);
	}

	[Fact]
	public void IEnumerableCtor_CopiesInOrder()
	{
		// Given
		IEnumerable<string> src = ["Read", "Eval", "Print", "Loop"];

		// When
		var d = new Deque<string>(src);

		// Then
		Assert.Equal(["Read", "Eval", "Print", "Loop"], [.. d]);
	}

	[Theory]
	[InlineData(7)]
	[InlineData(8)]
	[InlineData(10)]
	[InlineData(18)]
	[InlineData(24)]
	public void Reserve_AtLeastAdditional(int additional)
	{
		// Given
		var d = new Deque<int>();

		// When
		d.Reserve(additional);

		// Then
		Assert.True(d.Capacity >= additional);
	}

	[Theory]
	[InlineData(7)]
	[InlineData(8)]
	[InlineData(10)]
	[InlineData(18)]
	[InlineData(24)]
	public void ReserveExact_CapacityExact(int additional)
	{
		// Given
		var d = new Deque<int>();

		// When
		d.ReserveExact(additional);

		// Then
		Assert.Equal(additional, d.Capacity);
	}

	[Fact]
	public void Indexer_GetAndSet_CorrectLocation()
	{
		// Given
		var d = new Deque<int>();

		// When
		for (int i = 0; i < 5; i++)
			d.PushBack(i + 1);

		d[2] = 999;

		// Then
		Assert.Equal(1, d[0]);
		Assert.Equal(5, d[4]);
		Assert.Equal([1, 2, 999, 4, 5], [.. d]);
	}

	[Fact]
	public void Clear_EmptiesDeque()
	{
		// Given
		var d = new Deque<string>();

		// When
		d.PushBack("Apple");
		d.PushBack("Banana");
		d.PushFront("Orange");

		// Then
		Assert.Equal(3, d.Count);

		d.Clear();

		Assert.Equal(0, d.Count);
		Assert.True(d.IsEmpty);
		Assert.Throws<InvalidOperationException>(() => d.PeekFront());
		Assert.Throws<InvalidOperationException>(() => d.PeekBack());
		Assert.Empty(d.ToArray());
	}

	[Fact]
	public void Reserve_And_ReserveExact_DoNotChangeLogicalOrderOrCount()
	{
		// Given
		var d = new Deque<int>(capacity: 1);

		// When
		d.PushBack(2);
		d.Reserve(additional: 5);
		d.PushBack(3);
		d.ReserveExact(additional: 3);
		d.PushFront(1);

		// Then
		Assert.Equal(3, d.Count);

		d.PushBack(4);
		d.PushBack(5);
		d.PushFront(0);

		Assert.Equal([0, 1, 2, 3, 4, 5], [.. d]);
	}

	[Fact]
	public void ToArray_ElementsInOrder()
	{
		// Given
		var d = new Deque<int>();

		// When
		foreach (var x in new[] { 3, 1, 4, 1, 5 })
			d.PushBack(x);

		// Then
		Assert.Equal([3, 1, 4, 1, 5], [.. d]);
	}

	[Theory]
	[InlineData(0, 0)]
	[InlineData(0, 1)]
	[InlineData(1, 4)]
	[InlineData(2, 5)]
	[InlineData(0, 5)]
	public void ToArray_Range_CorrectOrderAndBounds(int start, int end)
	{
		// Given
		var d = new Deque<int>();

		// When
		for (int i = 0; i < 5; i++)
			d.PushBack(i + 1);

		var result = d.ToArray(start..end);

		var expected = new List<int>();
		for (int i = start; i < end; i++)
			expected.Add(i + 1);

		// Then
		Assert.Equal(expected, result);
	}

	[Fact]
	public void CopyTo_StartsAtGivenIndex()
	{
		// Given
		var d = new Deque<int>(3);

		// When
		d.PushBack(10);
		d.PushBack(20);
		d.PushBack(30);

		var array = new int[7];
		d.CopyTo(array, index: 2);

		// Then
		Assert.Equal([0, 0], array[0..2]);
		Assert.Equal([10, 20, 30], array[2..5]);
		Assert.Equal([0, 0], array[5..7]);
	}

	[Fact]
	public void AsSlices_ConcatenationMatchesToArray()
	{
		// Given
		var d = new Deque<int>(12);

		// When
		for (int i = 1; i <= 12; i++)
			d.PushBack(i);

		var range = 3..10;
		var expected = d.ToArray(range);

		var slices = d.AsSlices(range);
		var actual = slices.Front.ToArray().Concat(slices.Back.ToArray()).ToArray();

		// Then
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void WrapAround_PushThenPopThenPush_PreservesOrder()
	{
		// Given
		var d = new Deque<int>(capacity: 4);

		// When
		d.PushBack(1);
		d.PushBack(2);
		d.PushBack(3);
		d.PushBack(4);

		// Then
		// - PopFront twice removes from head, shifting logical head forward: [3,4]
		Assert.Equal(1, d.PopFront());
		Assert.Equal(2, d.PopFront());

		// - PushBack twice should wrap internally without corrupting logical order
		d.PushBack(5);
		d.PushBack(6);

		Assert.Equal([3, 4, 5, 6], [.. d]);
	}

	[Fact]
	public void WrapAround_AsSlicesMatchesToArrayWhenSplit()
	{
		// Given
		var d = new Deque<int>(capacity: 4);

		// When
		d.PushBack(1);
		d.PushBack(2);
		d.PushBack(3);
		d.PushBack(4);

		_ = d.PopFront();
		_ = d.PopFront(); // now [3,4]

		d.PushBack(5); // [3,4,5]
		d.PushFront(2); // [2,3,4,5]

		// Range hits both slices (front/back).
		var range = 1..4; // [3,4,5]

		// Then
		Assert.Equal([2, 3, 4, 5], [.. d]);

		var expected = d.ToArray(range);
		var slices = d.AsSlices(range);
		var actual = slices.Front.ToArray().Concat(slices.Back.ToArray()).ToArray();

		Assert.Equal(expected, actual);
	}

	[Fact]
	public void Contains_MatchesElements()
	{
		// Given
		var d = new Deque<string>(capacity: 6);

		// When
		d.PushBack("A");
		d.PushBack("B");
		d.PushBack("C");
		d.PushBack("D");
		d.PushBack("E");
		d.PushBack("F");

		// Then
		Assert.Contains("C", d);
		Assert.DoesNotContain("G", d);
		Assert.True(d.Contains("D"));
		Assert.False(d.Contains("H"));
	}
}
