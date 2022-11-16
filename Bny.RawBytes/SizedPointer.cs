namespace Bny.RawBytes;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
// wrapper for span that can be casted to object
internal unsafe readonly struct SizedPointer<T>
{
    public T* Ptr { get; init; }
    public int Size { get; init; }

    public SizedPointer(T* ptr, int size)
    {
        Ptr = ptr;
        Size = size;
    }

    public SizedPointer(ReadOnlySpan<T> span)
    {
        Size = span.Length;
        fixed (T* ptr = span)
            Ptr = ptr;
    }

    public Span<T> Span => new(Ptr, Size);
}

#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type