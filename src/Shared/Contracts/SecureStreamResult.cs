namespace SSDTLifecycleExtension.Shared.Contracts
{
    using System;
    using System.IO;
    using JetBrains.Annotations;

    public sealed class SecureStreamResult<T> : IDisposable
        where T : class
    {
        [CanBeNull] private readonly Stream _underlyingStream;

        [CanBeNull]
        public T Result { get; }

        [CanBeNull]
        public Exception Exception { get; }

        public SecureStreamResult([CanBeNull] Stream underlyingStream,
                                  [CanBeNull] T result,
                                  [CanBeNull] Exception exception)
        {
            _underlyingStream = underlyingStream;
            Result = result;
            Exception = exception;
        }

        public void Dispose()
        {
            _underlyingStream?.Dispose();
        }
    }
}