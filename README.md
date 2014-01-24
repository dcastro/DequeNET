# DequeNET

DequeNET (pronounced Deck Net) provides a concurrent lock-free deque C# implementation.
A deque, or double-ended queue, is a data structure that allows insertion and removal of items on both ends.
As such, `ConcurrentDeque<T>` supports 4 main operations (PushRight, PopRight, PushLeft and PopLeft) plus 2 others (PeekRight and PeekLeft).

The library also offers a simpler `Deque<T>` (not thread safe), implemented as a ring buffer.
This implementation allows all 6 operations to be executed in constant time O(1).


### The Algorithm

The implementation is based on the algorithm proposed by Maged M. Michael [1].
The algorithm uses the atomic primitive CAS (compare-and-swap) to achieve lock-freedom.
Because of this property, the algorithm guarantees system-wide progress (i.e., an operation will always complete within a finite number of steps) and is immune to deadlocks, unlike traditional mutual exclusion techniques.

Without contention, all four main operations run in constant time O(1).
Under contention by P processes, the operations' total work is O(P).
PeekRight and PeekLeft run in constant time regardless of contention.


[1] Michael, Maged, 2003, CAS-Based Lock-Free Algorithm for Shared Deques, *Euro-Par 2003 Parallel Processing*, v. 2790, p. 651-660, http://www.research.ibm.com/people/m/michael/europar-2003.pdf (Decembre 22, 2013).


### NuGet

To install DequeNET, run the following command in the Package Manager Console

```
PM> Install-Package DequeNET
```

