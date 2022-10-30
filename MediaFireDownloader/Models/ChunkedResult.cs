using System;

namespace MediaFireDownloader.Models
{
    internal sealed class ChunkedResult<T>
    {
        public ChunkedResult(T[] result, bool hasNextChunk)
        {
            Result = result;
            HasNextChunk = hasNextChunk;
        }

        public T[] Result { get; }
        public bool HasNextChunk { get; }
    }
}
