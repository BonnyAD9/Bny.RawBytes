namespace Bny.RawBytes;

internal static class Helpers
{
    public static int LenIndexOf(this ReadOnlySpan<byte> span, ReadOnlySpan<byte> value)
    {
        for (int i = 0; i < span.Length; i += value.Length)
        {
            if (span[i..].StartsWith(value))
                return i;
        }
        return -1;
    }
}
