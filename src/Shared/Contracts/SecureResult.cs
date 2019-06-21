namespace SSDTLifecycleExtension.Shared.Contracts
{
    using System;
    using JetBrains.Annotations;

    public class SecureResult<T>
        where T : class
    {
        [CanBeNull]
        public T Value { get; }

        [CanBeNull]
        public Exception Exception { get; }

        public SecureResult([CanBeNull] T value,
                            [CanBeNull] Exception exception)
        {
            Value = value;
            Exception = exception;
        }
    }
}