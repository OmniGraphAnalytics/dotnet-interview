namespace OmniGraphInterview.Utils;

/// <summary>
/// A heap that keeps the top N elements based on a comparison function.
/// While the heap is not full, it will add elements to the heap.
/// When the heap is full, it will only add elements that are greater than the smallest element in the heap, popping the smallest element if necessary.
/// </summary>
/// <typeparam name="T"></typeparam>
public class TopNHeap<T> : List<T>
{
    private readonly Func<T, T, int> compare;

    /// <summary>
    /// Create a new TopNHeap with the given size and comparison function.
    /// </summary>
    /// <param name="size"></param>
    /// <param name="compare"></param>
    public TopNHeap(int size, Func<T, T, int> compare) : base(size)
    {
        Size = size;
        this.compare = compare;
    }

    /// <summary>
    /// Returns true if the heap has limited the number of elements.
    /// </summary>
    public bool HasOverflow => UnderlyingCount > Size;

    /// <summary>
    /// Returns the number of elements added to the heap, regardless of heap size.
    /// </summary>
    public int UnderlyingCount { get; private set; } = 0;

    /// <summary>
    /// Returns the size of the heap.
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// Returns true if the heap is locked and no more elements can be added.
    /// </summary>
    public bool IsLocked { get; init; } = false;

    /// <summary>
    /// Add an item to the heap.
    /// </summary>
    /// <param name="item"></param>
    public new void Add(T item)
    {
        if (IsLocked)
        {
            throw new InvalidOperationException("Heap is locked and cannot be modified.");
        }
        if (Count < Size)
        {
            base.Add(item);
            Sort((a, b) => compare(arg1: b, arg2: a));
        }
        else if (compare(arg1: item, arg2: base[^1]) > 0)
        {
            base[^1] = item;
            Sort((a, b) => compare(arg1: b, arg2: a));
        }

        UnderlyingCount++;
    }

    private void FromOrderedArray(T[] array, int underlyingCount)
    {
        Clear();
        AddRange(array);
        UnderlyingCount = underlyingCount;
    }

    /// <summary>
    /// Perform an async map operation on the heap.
    /// Note, this operation will render the heap locked as the comparer is no longer valid.
    /// </summary>
    /// <param name="selector"></param>
    /// <returns></returns>
    public async Task<TopNHeap<TOut>> SelectAsync<TOut>(Func<T, Task<TOut>> selector)
    {
        var result = new TopNHeap<TOut>(size: Size, compare: (_, _) => 0) {IsLocked = true};
        var mapped = await Task.WhenAll(this.Select(selector));
        result.FromOrderedArray(array: mapped, underlyingCount: UnderlyingCount);
        return result;
    }
}
