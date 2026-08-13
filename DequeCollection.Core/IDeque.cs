// Represents a double-ended queue containing elements of type `T`,
// providing push and pop operations on either end.
public interface IDeque<T> : IEnumerable<T>
{
	// Returns the number of elements contained in the IDeque<T>.
	int Count { get; }

	// Returns a value indicating whether the IDeque<T> is empty.
	bool IsEmpty { get; }

	// Prepends an element to the deque.
	void PushFront(T value);

	// Appends an element to the deque.
	void PushBack(T value);

	// Removes the first element and returns it.
	// May throw if the IDeque<T> is empty.
	T PopFront();

	// Removes the last element and returns it.
	// May throw if the IDeque<T> is empty.
	T PopBack();
}
