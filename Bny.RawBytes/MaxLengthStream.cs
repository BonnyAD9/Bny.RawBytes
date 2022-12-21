using System.Diagnostics;

namespace Bny.RawBytes;

internal class MaxLengthStream : Stream
{
    readonly Stream _stream;
    public long MaxLength { get; init; }
    public long CurPos { get; private set; }
    public bool FakeLengths { get; init; }

    public MaxLengthStream(Stream stream, int maxLength, bool fakeLengths = false)
    {
        _stream = stream;
        MaxLength = maxLength;
        FakeLengths = fakeLengths;
    }

    public override bool CanRead => _stream.CanRead;

    public override bool CanSeek => _stream.CanSeek;

    public override bool CanWrite => _stream.CanWrite;

    public override long Length => MaxLength;

    public override long Position
    {
        get => CurPos;
        set
        {
            if (value < 0 || value > CurPos)
                throw new ArgumentOutOfRangeException(nameof(value));
            _stream.Position = _stream.Position - CurPos + value;
            CurPos = value;
        }
    }

    public override void Flush()
    {
        _stream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var toRead = (int)Math.Min(count, MaxLength - CurPos);
        int res = _stream.Read(
            buffer                                  ,
            offset                                  ,
            (int)Math.Min(count, MaxLength - CurPos));
        CurPos += res;
        return FakeLengths && toRead == res ? count : res;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                if (offset < 0 || offset > MaxLength)
                    throw new ArgumentOutOfRangeException(nameof(offset));
                _stream.Seek(-CurPos + offset, SeekOrigin.Current);
                CurPos = offset;
                return CurPos;
            case SeekOrigin.Current:
                if (CurPos + offset < 0 || CurPos + offset > MaxLength)
                    throw new ArgumentOutOfRangeException(nameof(offset));
                _stream.Seek(offset, SeekOrigin.Current);
                CurPos += offset;
                return CurPos;
            case SeekOrigin.End:
                if (offset > 0 || offset + CurPos < 0)
                    throw new ArgumentOutOfRangeException(nameof(offset));
                _stream.Seek(MaxLength + offset, SeekOrigin.Current);
                CurPos = MaxLength + offset;
                return CurPos;
        }
        throw new UnreachableException();
    }

    public override void SetLength(long value)
        => throw new InvalidOperationException();
    
    public override void Write(byte[] buffer, int offset, int count)
    {
        _stream.Write(
            buffer                                  ,
            offset                                  ,
            (int)Math.Min(count, MaxLength - CurPos));
        CurPos += count;
    }
}
