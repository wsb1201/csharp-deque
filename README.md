# Generic class implementation of a double-ended queue

A **double-ended queue** (deque) is a collection that supports insertion and removal at **both ends**.

This `Deque<T>` implementation uses a **growable ring buffer**, so elements wrap around a circular buffer while still offering amortized O(1) push/pop operations at either end.

The `IDeque<T>` interface provides basic interoperability, exposing a subset of the `Deque<T>` API.

## API overview

### Core properties

- `Count` — number of elements
- `Capacity` — backing buffer capacity
- `IsFull` / `IsEmpty`
- `IsContiguous` — whether the deque’s contents are stored contiguously (as a single span)
- `this[int index]` — random access by logical index

### Views / bulk helpers

- `PeekFront()` / `PeekBack()` — read without removing (throws on empty)
- `AsSlices()` — returns contents as two spans: `Front` and `Back`
- `ToArray()` — materializes the deque (or a slice range) into a new array
- `CopyTo()` — copies contents into an existing array

### Capacity management

- `Reserve(int additional)` — reserve at least `additional` more slots (may over-allocate)
- `ReserveExact(int additional)` — reserve exactly `additional` more slots

### Mutating operations

- `Clear()` — removes all elements
- `PushFront(T value)` / `PushBack(T value)` — insert at either end (allocating when full)
- `PopFront()` / `PopBack()` — remove and return from either end (throws on empty)
- `TryPopFront()` / `TryPopBack()` — remove/return or fail if empty

## Implementation notes

- Storage is delegated to a `RingBuffer<T>` with a logical `Head` index and a `Count`.
- `PushFront` moves `Head` backward (wrapping), writes the value, and increments `Count`.
- `PushBack` writes at the computed wrapped index and increments `Count`.
- `PopFront` reads at `Head`, advances `Head`, clears the slot, and decrements `Count`.
- `PopBack` computes the wrapped index of the last element, clears it, and decrements `Count`.
- Whenever the buffer needs more space, reallocation is done before inserting.
- Methods like `AsSlices` expose logical contents as `Front`/`Back` spans based on how the ring wraps.
