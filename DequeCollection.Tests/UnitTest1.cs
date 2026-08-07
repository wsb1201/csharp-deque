using DequeCollection.Core;

namespace DequeCollection.Tests;

public class UnitTest1
{
	[Fact]
	public void Test_Deque_PushPopFront()
	{
		var deq = new Deque<int>();

		deq.PushFront(1);
		deq.PushFront(2);
		deq.PushFront(3);

		Assert.Equal(3, deq.PopFront());
		Assert.Equal(2, deq.PopFront());
		Assert.Equal(1, deq.PopFront());
	}

	[Fact]
	public void Test_Deque_PushPopBack()
	{
		var deq = new Deque<int>();

		deq.PushBack(1);
		deq.PushBack(2);
		deq.PushBack(3);

		Assert.Equal(3, deq.PopBack());
		Assert.Equal(2, deq.PopBack());
		Assert.Equal(1, deq.PopBack());
	}
}
