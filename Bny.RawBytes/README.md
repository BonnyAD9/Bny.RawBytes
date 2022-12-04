# Bny.RawBytes
This directory contains the library source code.

## In this directory
- **Bytes.cs:** contains generic stuff in the `Bytes` static class
- **Bytes.To.Span:** contains the span versions of the `Bytes.To` methods
- **Bytes.To.Stream:** contains the stream versions of the `Bytes.To` methods
- **Bytes.From.Span:** contains the span versions of the `Bytes.From` methods
- **Bytes.From.Stream:** contains the stream versions of the `Bytes.From` methods
- **BinaryObjectAttribute.cs:** contains the `BinaryObjectAttribute` for marking types that can be converted to binary
- **BinaryMemberAttribute.cs:** contains the `BinaryMemberAttribute` for marking properties and fields that specify the conversion to binary
- **BinaryPaddingAttribute.cs:** contains the `BinaryPaddingAttribute` for specifying padding
- **BinaryExactAttribute.cs:** contains the `BinaryExactAttribute` for specifying exact binary data
- **BytesParam.cs:** contains the `BytesParam` record for passing info to the `Bytes.From` and `Bytes.To` methods about the conversion
- **Endianness.cs:** contains the `Endianness` enum that represents the endianness of a conversion
- **Sign.cs:** contains the `Sign` enum that specifies whether number should be signed or not
- **BinaryEncoding.cs:** contains the `BinaryEncoding` abstract class that represents encoding for binary conversion
- **IBinaryObject.cs:** contains the `IBinaryObject` interface
- **IBinaryObjectStream.cs:** contains the `IBinaryObjectStream` interface
- **NetBinaryEncoding.cs:** contains the `NetBinaryEncoding` class
- **IBinaryObjectWrite.cs:** contains the `IBinaryObjectWrite` interface that should not be implemented by itself
- **BinaryAttributeInfo.cs:** contains the iternal `BinaryAttributeInfo` class that helps interpreting the `BinaryAttribute`s
- **MaxLengthStream.cs:** contains stream that limits the underlaing stream size
