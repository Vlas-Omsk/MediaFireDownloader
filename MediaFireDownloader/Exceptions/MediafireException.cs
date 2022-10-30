using System;

namespace MediaFireDownloader
{
    internal sealed class MediafireException : Exception
    {
        public int ErrorCode { get; }

        public MediafireException(int errorCode, string message) : base(message + " (" + errorCode + ")")
        {
            ErrorCode = errorCode;
        }
    }
}
