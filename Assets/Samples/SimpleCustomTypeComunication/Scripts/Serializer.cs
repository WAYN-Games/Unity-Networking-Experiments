using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.IO.Compression;
using Unity.Collections.LowLevel.Unsafe;

public struct Serializer<T> where T : struct
{
    int size;
    
    public void Initialize()
    {
        size = UnsafeUtility.SizeOf<T>();
    }

    public byte[] Serialize(T obj)
    {
        using (var memoryStream = new MemoryStream(size))
        {
            var binaryFormatter = new BinaryFormatter();

            binaryFormatter.Serialize(memoryStream, obj);

            return Compress(memoryStream.ToArray());
        }
    }

    public T DeSerialize(byte[] arrBytes)
    {
        using (var memoryStream = new MemoryStream(size))
        {
            var binaryFormatter = new BinaryFormatter();
            var decompressed = Decompress(arrBytes);

            memoryStream.Write(decompressed, 0, decompressed.Length);
            memoryStream.Seek(0, SeekOrigin.Begin);

            return (T)binaryFormatter.Deserialize(memoryStream);
        }
    }

    private byte[] Compress(byte[] input)
    {
        byte[] compressesData;

        using (var outputStream = new MemoryStream(size))
        {
            using (var zip = new GZipStream(outputStream, CompressionMode.Compress))
            {
                zip.Write(input, 0, input.Length);
            }

            compressesData = outputStream.ToArray();
        }

        return compressesData;
    }

    private byte[] Decompress(byte[] input)
    {
        byte[] decompressedData;

        using (var outputStream = new MemoryStream(size))
        {
            using (var inputStream = new MemoryStream(input))
            {
                using (var zip = new GZipStream(inputStream, CompressionMode.Decompress))
                {
                    zip.CopyTo(outputStream);
                }
            }

            decompressedData = outputStream.ToArray();
        }

        return decompressedData;
    }
}