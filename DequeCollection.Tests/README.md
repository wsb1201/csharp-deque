# Test overview

This test suite verifies the core behavior and invariants of `Deque<T>`. It checks the deque's initial state, performs queue operations on both ends while testing for correct ordering semantics, and ensures consistent behavior through enumeration and conversion APIs.

## How to run the tests

From the repository root (where the solution/project file is located):

```bash
dotnet test
```

This will build the project and run all tests in [DequeTests.cs](./DequeTests.cs).

## What is tested

### Constructors & initial state

- Instances have the expected properties after initialization.
- Allocated buffers have the requested capacity.
- Copy constructors preserve logical order from source.

### Deque operations

- Push/Pop operations correctly produce FIFO/LIFO behavior depending on which ends you push and pop from.
- Push/Pop operations increment/decrement Count correctly.
- Push operations maintain the correct logical order when the capacity is increased.
- An empty/cleared deque makes the peek operations fail as expected.

### Edge cases and error handling

- PopFront/PopBack throw on empty.
- TryPopFront/TryPopBack return false and default values on empty.
- Indexing into the deque throws when the given range is out of bounds.

### IEnumerator interface contracts

- Enumerator iterates in logical order.
- Enumerator throws when:
-   - accessed via Current before MoveNext,
-   - accessed via Current after enumeration ends,
-   - the deque is modified during enumeration.

### Data access and wrap-around behavior

- Indices are mapped to the correct memory locations.
- CopyTo writes into destination starting at the requested index.
- When the internal buffer wraps, the correct logical order is preserved.
- All relevant methods behave as expected when accessing non-contiguous memory.
