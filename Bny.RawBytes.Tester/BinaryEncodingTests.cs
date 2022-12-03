namespace Bny.RawBytes.Tests;


// this also implicitly tests NetBinaryEncoding
[UnitTest]
internal class BinaryEncodingTests
{
    [UnitTest]
    public static void Test_DefaultEncodings(Asserter a)
    {
        a.Assert(BinaryEncoding.TryGet("us-ascii") is not null);
        a.Assert(BinaryEncoding.TryGet("iso-8859-1") is not null);
        a.Assert(BinaryEncoding.TryGet("utf-8") is not null);
        a.Assert(BinaryEncoding.TryGet("utf-16") is not null);
        a.Assert(BinaryEncoding.TryGet("utf-16BE") is not null);
        a.Assert(BinaryEncoding.TryGet("utf-32") is not null);
        a.Assert(BinaryEncoding.TryGet("utf-32BE") is not null);
    }

    [UnitTest]
    public static void Test_NullTerminator(Asserter a)
    {
        var ascii = BinaryEncoding.TryGet("us-ascii")!;

        a.Assert(ascii.NullTerminator.Length == 1);
        a.Assert(ascii.NullTerminator[0] == 0);

        var utf16 = BinaryEncoding.TryGet("utf-16")!;

        a.Assert(utf16.NullTerminator.Length == 2);
        a.Assert(utf16.NullTerminator.StartsWith("\0\0"u8));

        var utf32 = BinaryEncoding.TryGet("utf-32")!;

        a.Assert(utf32.NullTerminator.Length == 4);
        a.Assert(utf32.NullTerminator.StartsWith("\0\0\0\0"u8));
    }

    [UnitTest]
    public static void Test_GetString(Asserter a)
    {
        var utf8 = BinaryEncoding.TryGet("utf-8")!;

        a.Assert(utf8.GetString("Hello\0World"u8) == "Hello\0World");

        a.Assert(utf8.GetString(
            "Hello\0World"u8,
            out int spanReadedBytes) == "Hello");

        a.Assert(spanReadedBytes == "Hello\0".Length);

        a.Assert(
            utf8.GetString(new MemoryStream("Hello\0World"u8.ToArray())) ==
            "Hello\0World");

        a.Assert(utf8.GetString(new MemoryStream("Hello\0World"u8.ToArray()),
            out int streamReadedBytes) == "Hello");

        a.Assert(streamReadedBytes== "Hello\0".Length);
    }

    [UnitTest]
    public static void Test_GetBytes(Asserter a)
    {
        var utf8 = BinaryEncoding.TryGet("utf-8")!;

        a.Assert(Check.AreSame(
            utf8.GetBytes("Hello\0World"),
            "Hello\0World"u8.ToArray()));
    }

    [UnitTest]
    public static void Test_AllEncodings(Asserter a)
    {
        foreach (var e in BinaryEncoding.Encodings.Values)
        {
            a.Assert(true, $"Tested encoding: {e.Name}");
            var bytes = e.GetBytes("Hello\0World");

            a.Assert(e.GetString(bytes) == "Hello\0World");
            a.Assert(e.GetString(bytes, out _) == "Hello");

            a.Assert(e.GetString(new MemoryStream(bytes, false)) ==
                "Hello\0World");

            a.Assert(e.GetString(new MemoryStream(bytes, false), out _) ==
                "Hello");
        }
    }
}
