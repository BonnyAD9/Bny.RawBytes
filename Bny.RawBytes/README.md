# Bny.RawBytes
This directory contains the library source code.

## In this directory
- **Bytes.cs:** contains the Bytes` static class (contains for example the `Bytes.To` and `Bytes.From` methods)
- **BinaryObjectAttribute.cs:** contains the `BinaryObjectAttribute` for marking types that can be converted to binary
- **BinaryMemberAttribute.cs:** contains the `BinaryMemberAttribute` for marking properties and fields that specify the conversion to binary
- **BytesParam.cs:** contains the `BytesParam` record for passing info to the `Bytes.From` and `Bytes.To` methods about the conversion
- **Endianness.cs:** contains the `Endianness` enum that represents the endianness of a conversion
- **Sign.cs:** contains the `Sign` enum that specifies whether number should be signed or not
- **BinaryEncoding.cs:** contains the `BinaryEncoding` abstract class that represents encoding for binary conversion
- **IBinaryObject.cs:** contains the `IBinaryObject` interface
- **IBinaryObjectStream.cs:** contains the `IBinaryObjectStream` interface
- **NetBinaryEncoding.cs:** contains the `NetBinaryEncoding` class
- **IBinaryObjectWrite.cs:** contains the `IBinaryObjectWrite` interface that should not be implemented by itself
- **BinaryMemberAttributeInfo.cs:** contains the iternal `BinaryMemberAttributeInfo` class that helps interpreting the `BinaryMemberAttribute`
- **Helpers.cs:** contains internal static methods that are needed by some internal tasks
- **SizedPointer.cs:** contains internal struct `SizedPointer` that wraps span and can be boxed
